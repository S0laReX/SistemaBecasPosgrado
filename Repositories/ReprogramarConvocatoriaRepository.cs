using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace SistemaBecasWeb.Repositories;

public class ReprogramarConvocatoriaRepository(IConfiguration configuration)
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
            "SELECT COUNT(*) FROM user_objects WHERE object_name = 'SP_REPROGRAMAR_CONVOCATORIA' AND object_type = 'PROCEDURE' AND status = 'VALID'", cn);
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    public void Reprogramar(int idOferta, DateTime nuevaFechaInicio, DateTime nuevaFechaFin)
    {
        using var cn = Conexion();
        cn.Open();
        using var tx = cn.BeginTransaction();
        using var cmd = cn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "sp_reprogramar_convocatoria";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;
        cmd.Parameters.Add("p_id_oferta", OracleDbType.Int32).Value = idOferta;
        cmd.Parameters.Add("p_nueva_fecha_inicio", OracleDbType.Date).Value = nuevaFechaInicio.Date;
        cmd.Parameters.Add("p_nueva_fecha_fin", OracleDbType.Date).Value = nuevaFechaFin.Date;

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