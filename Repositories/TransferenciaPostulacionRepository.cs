using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace SistemaBecasWeb.Repositories;

public class TransferenciaPostulacionRepository(IConfiguration configuration)
{
    private OracleConnection Conexion()
    {
        var connectionString = configuration.GetConnectionString("OracleDB");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Configure la conexión OracleDB localmente.");
        return new OracleConnection(connectionString);
    }

    public bool EstaInstalado()
    {
        using var cn = Conexion();
        cn.Open();
        using var cmd = new OracleCommand(
            "SELECT COUNT(*) FROM user_objects WHERE object_name = 'SP_TRANSFERIR_POSTULACION' AND object_type = 'PROCEDURE' AND status = 'VALID'", cn);
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    public void Transferir(int solicitudId, int ofertaDestinoId)
    {
        using var cn = Conexion();
        cn.Open();
        using var tx = cn.BeginTransaction();
        using var cmd = cn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "sp_transferir_postulacion";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;
        cmd.Parameters.Add("p_solicitud", OracleDbType.Int32).Value = solicitudId;
        cmd.Parameters.Add("p_oferta_destino", OracleDbType.Int32).Value = ofertaDestinoId;

        try
        {
            cmd.ExecuteNonQuery();
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
