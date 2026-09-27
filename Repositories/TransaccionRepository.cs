using System.Data;
using Oracle.ManagedDataAccess.Client;
using SistemaBecasWeb.Models;

namespace SistemaBecasWeb.Repositories;

public class TransaccionRepository(IConfiguration configuration)
{
    private OracleConnection Conexion()
    {
        var connectionString = configuration.GetConnectionString("OracleDB");
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("Configure OracleDB localmente.");
        return new(connectionString);
    }

    public PanelBecasViewModel Consultar()
    {
        using var cn = Conexion();
        cn.Open();
        var panel = new PanelBecasViewModel();
        using var check = new OracleCommand("SELECT COUNT(*) FROM user_objects WHERE object_name='PKG_BECAS' AND object_type IN ('PACKAGE','PACKAGE BODY') AND status='VALID'", cn);
        panel.Disponible = Convert.ToInt32(check.ExecuteScalar()) == 2;
        using var ofertas = new OracleCommand("SELECT o.id_oferta, o.ref_programa.nombre FROM ofertas o WHERE o.estado_oferta='Vigente' ORDER BY o.id_oferta", cn);
        using (var rd = ofertas.ExecuteReader())
            while (rd.Read()) panel.Ofertas.Add(new(rd.GetInt32(0), $"#{rd.GetInt32(0)} · {rd.GetValue(1)}"));
        using var solicitudes = new OracleCommand("SELECT s.id_solicitud, s.ref_postulante.nombres, s.ref_oferta.id_oferta FROM solicitudes s WHERE s.estado='Pendiente' ORDER BY s.id_solicitud", cn);
        using (var rd = solicitudes.ExecuteReader())
            while (rd.Read()) panel.Solicitudes.Add(new(rd.GetInt32(0), $"#{rd.GetInt32(0)} · {rd.GetValue(1)} · Oferta {rd.GetValue(2)}"));
        return panel;
    }

    public void Ejecutar(TransaccionViewModel m)
    {
        using var cn = Conexion();
        cn.Open();
        using var tx = cn.BeginTransaction();
        using var cmd = cn.CreateCommand();
        cmd.Transaction = tx;
        cmd.BindByName = true;
        cmd.CommandType = CommandType.StoredProcedure;
        void P(string nombre, OracleDbType tipo, object? valor) => cmd.Parameters.Add(nombre, tipo).Value = valor ?? DBNull.Value;
        switch (m.Operacion)
        {
            case "candidato":
                cmd.CommandText = "pkg_becas.registrar_candidato";
                P("p_doc", OracleDbType.Varchar2, m.Documento?.Trim());
                P("p_nombres", OracleDbType.Varchar2, m.Nombres); P("p_apellidos", OracleDbType.Varchar2, m.Apellidos);
                P("p_correo", OracleDbType.Varchar2, m.Correo); P("p_nivel", OracleDbType.Varchar2, m.Nivel); break;
            case "experiencia":
                cmd.CommandText = "pkg_becas.agregar_experiencia";
                P("p_doc", OracleDbType.Varchar2, m.Documento?.Trim()); P("p_empresa", OracleDbType.Varchar2, m.Empresa);
                P("p_cargo", OracleDbType.Varchar2, m.Cargo); P("p_inicio", OracleDbType.Date, m.Inicio); P("p_fin", OracleDbType.Date, m.Fin); break;
            case "postular":
                cmd.CommandText = "pkg_becas.postular";
                P("p_doc", OracleDbType.Varchar2, m.Documento?.Trim()); P("p_oferta", OracleDbType.Int32, m.Oferta);
                P("p_resumen", OracleDbType.Varchar2, m.Resumen); break;
            case "resolver":
                cmd.CommandText = "pkg_becas.resolver";
                P("p_solicitud", OracleDbType.Int32, m.Solicitud); P("p_estado", OracleDbType.Varchar2, m.Estado); break;
            case "cerrar":
                cmd.CommandText = "pkg_becas.cerrar_oferta"; P("p_oferta", OracleDbType.Int32, m.Oferta); break;
            default: throw new ArgumentException("Operación desconocida.");
        }
        try { cmd.ExecuteNonQuery(); tx.Commit(); }
        catch { tx.Rollback(); throw; }
    }
}
