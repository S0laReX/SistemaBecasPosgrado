using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace SistemaBecasWeb.Repositories;

public class CorreoPostulanteRepository(IConfiguration configuration)
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
            "SELECT COUNT(*) FROM user_objects WHERE object_name = 'SP_ACTUALIZAR_CORREO_POSTULANTE' AND object_type = 'PROCEDURE' AND status = 'VALID'", cn);
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    public void Actualizar(string documento, string correoNuevo)
    {
        using var cn = Conexion();
        cn.Open();
        using var tx = cn.BeginTransaction();
        using var cmd = cn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "sp_actualizar_correo_postulante";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;
        cmd.Parameters.Add("p_doc", OracleDbType.Varchar2).Value = documento.Trim();
        cmd.Parameters.Add("p_correo_nuevo", OracleDbType.Varchar2).Value = correoNuevo.Trim();

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
