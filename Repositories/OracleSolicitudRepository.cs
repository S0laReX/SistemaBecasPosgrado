using SistemaBecasWeb.Models;  
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace SistemaBecasWeb.Repositories
{
    public class OracleSolicitudRepository : ISolicitudRepository
    {
        private readonly string _connectionString;

        public OracleSolicitudRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDB");
        }

        public List<OfertaViewModel> ObtenerOfertasVigentes()
        {
            List<OfertaViewModel> lista = new List<OfertaViewModel>();

            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                // Navegación de objetos usando el modelo Objeto-Relacional
                string sql = @"SELECT o.id_oferta, o.ref_programa.nombre AS nombre_programa 
               FROM SYSTEM.ofertas o 
               WHERE o.estado_oferta = 'Vigente'";

                OracleCommand cmd = new OracleCommand(sql, conn);
                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new OfertaViewModel
                        {
                            IdOferta = Convert.ToInt32(reader["id_oferta"]),
                            NombrePrograma = reader["nombre_programa"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public string RegistrarSolicitud(string docIdentidad, int idOferta, string resumen)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand("fn_registrar_solicitud", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 1. Parámetro de Retorno SIEMPRE VA PRIMERO
                    OracleParameter retVal = new OracleParameter("RetVal", OracleDbType.Varchar2, 500);
                    retVal.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(retVal);

                    // 2. Parámetros de Entrada
                    cmd.Parameters.Add("p_doc_identidad", OracleDbType.Varchar2).Value = docIdentidad;
                    cmd.Parameters.Add("p_id_oferta", OracleDbType.Decimal).Value = idOferta;

                    // 3. Parámetro CLOB para textos largos
                    cmd.Parameters.Add("p_resumen", OracleDbType.Clob).Value = resumen;

                    cmd.ExecuteNonQuery();
                    return retVal.Value.ToString();
                }
            }
        }

        public int ContarAceptados(int idOferta)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand("fn_contar_aceptados", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    OracleParameter retVal = new OracleParameter("RetVal", OracleDbType.Decimal);
                    retVal.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(retVal);

                    cmd.Parameters.Add("p_id_oferta", OracleDbType.Decimal).Value = idOferta;

                    cmd.ExecuteNonQuery();
                    // Castear el resultado Decimal de Oracle a int de C#
                    return Convert.ToInt32(retVal.Value.ToString());
                }
            }
        }

        public List<UniversidadViewModel> ObtenerUniversidades()
        {
            List<UniversidadViewModel> lista = new List<UniversidadViewModel>();
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                // Consulta simple a la tabla de objetos
                string sql = "SELECT id_universidad, nombre, pais, ciudad FROM universidades";
                OracleCommand cmd = new OracleCommand(sql, conn);
                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new UniversidadViewModel
                        {
                            IdUniversidad = Convert.ToInt32(reader["id_universidad"]),
                            Nombre = reader["nombre"].ToString(),
                            Pais = reader["pais"].ToString(),
                            Ciudad = reader["ciudad"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
        public UniversidadViewModel ObtenerUniversidadPorId(int id)
        {
            UniversidadViewModel uni = new UniversidadViewModel();
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = "SELECT * FROM universidades WHERE id_universidad = :id";
                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("id", OracleDbType.Decimal).Value = id;
                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        uni.IdUniversidad = Convert.ToInt32(reader["id_universidad"]);
                        uni.Nombre = reader["nombre"].ToString();
                        uni.Pais = reader["pais"].ToString();
                        uni.Ciudad = reader["ciudad"].ToString();
                        uni.Direccion = reader["direccion"].ToString();
                        uni.Telefono = reader["telefono"].ToString();
                    }
                }
            }
            return uni;
        }

        public void CrearUniversidad(UniversidadViewModel uni)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                // Generamos un ID manual simple simulando un autoincremental
                string sqlId = "SELECT NVL(MAX(id_universidad), 0) + 1 FROM universidades";
                OracleCommand cmdId = new OracleCommand(sqlId, conn);
                conn.Open();
                int nuevoId = Convert.ToInt32(cmdId.ExecuteScalar());

                // INSERT usando el constructor del objeto
                string sql = @"INSERT INTO universidades VALUES (
                        t_universidad(:id, :nom, :pais, :ciu, :dir, :tel)
                      )";
                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("id", OracleDbType.Decimal).Value = nuevoId;
                cmd.Parameters.Add("nom", OracleDbType.Varchar2).Value = uni.Nombre;
                cmd.Parameters.Add("pais", OracleDbType.Varchar2).Value = uni.Pais;
                cmd.Parameters.Add("ciu", OracleDbType.Varchar2).Value = uni.Ciudad;
                cmd.Parameters.Add("dir", OracleDbType.Varchar2).Value = uni.Direccion;
                cmd.Parameters.Add("tel", OracleDbType.Varchar2).Value = uni.Telefono;

                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarUniversidad(UniversidadViewModel uni)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                // En objetos, actualizamos usando un alias (u)
                string sql = @"UPDATE universidades u 
                       SET u.nombre = :nom, u.pais = :pais, u.ciudad = :ciu, 
                           u.direccion = :dir, u.telefono = :tel 
                       WHERE u.id_universidad = :id";
                OracleCommand cmd = new OracleCommand(sql, conn);
                conn.Open();
                cmd.Parameters.Add("nom", OracleDbType.Varchar2).Value = uni.Nombre;
                cmd.Parameters.Add("pais", OracleDbType.Varchar2).Value = uni.Pais;
                cmd.Parameters.Add("ciu", OracleDbType.Varchar2).Value = uni.Ciudad;
                cmd.Parameters.Add("dir", OracleDbType.Varchar2).Value = uni.Direccion;
                cmd.Parameters.Add("tel", OracleDbType.Varchar2).Value = uni.Telefono;
                cmd.Parameters.Add("id", OracleDbType.Decimal).Value = uni.IdUniversidad;

                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarUniversidad(int id)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = "DELETE FROM universidades WHERE id_universidad = :id";
                OracleCommand cmd = new OracleCommand(sql, conn);
                conn.Open();
                cmd.Parameters.Add("id", OracleDbType.Decimal).Value = id;
                cmd.ExecuteNonQuery();
            }
        }

        public List<ProgramaViewModel> ObtenerProgramas()
        {
            List<ProgramaViewModel> lista = new List<ProgramaViewModel>();
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                // Navegamos por p.ref_universidad para traer los datos de la otra tabla
                string sql = @"SELECT p.cod_programa, p.nombre, p.descripcion, p.area, 
                              p.tipo_programa, p.modalidad, 
                              p.ref_universidad.id_universidad AS id_uni, 
                              p.ref_universidad.nombre AS nombre_uni 
                       FROM programas p";

                OracleCommand cmd = new OracleCommand(sql, conn);
                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new ProgramaViewModel
                        {
                            CodPrograma = reader["cod_programa"].ToString(),
                            Nombre = reader["nombre"].ToString(),
                            Descripcion = reader["descripcion"].ToString(),
                            Area = reader["area"].ToString(),
                            TipoPrograma = reader["tipo_programa"].ToString(),
                            Modalidad = reader["modalidad"].ToString(),
                            IdUniversidad = Convert.ToInt32(reader["id_uni"]),
                            NombreUniversidad = reader["nombre_uni"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public ProgramaViewModel ObtenerProgramaPorCod(string cod)
        {
            ProgramaViewModel prog = new ProgramaViewModel();
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = @"SELECT p.cod_programa, p.nombre, p.descripcion, p.area, 
                              p.tipo_programa, p.modalidad, 
                              p.ref_universidad.id_universidad AS id_uni 
                       FROM programas p WHERE p.cod_programa = :cod";

                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("cod", OracleDbType.Varchar2).Value = cod;
                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        prog.CodPrograma = reader["cod_programa"].ToString();
                        prog.Nombre = reader["nombre"].ToString();
                        prog.Descripcion = reader["descripcion"].ToString();
                        prog.Area = reader["area"].ToString();
                        prog.TipoPrograma = reader["tipo_programa"].ToString();
                        prog.Modalidad = reader["modalidad"].ToString();
                        prog.IdUniversidad = Convert.ToInt32(reader["id_uni"]);
                    }
                }
            }
            return prog;
        }

        public void CrearPrograma(ProgramaViewModel prog)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = @"INSERT INTO programas VALUES (
                        t_programa(:cod, :nom, :desc, :area, :tipo, :mod, 
                            (SELECT REF(u) FROM universidades u WHERE u.id_universidad = :idUni)
                        )
                      )";

                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("cod", OracleDbType.Varchar2).Value = prog.CodPrograma;
                cmd.Parameters.Add("nom", OracleDbType.Varchar2).Value = prog.Nombre;
                cmd.Parameters.Add("desc", OracleDbType.Varchar2).Value = prog.Descripcion;
                cmd.Parameters.Add("area", OracleDbType.Varchar2).Value = prog.Area;
                cmd.Parameters.Add("tipo", OracleDbType.Varchar2).Value = prog.TipoPrograma;
                cmd.Parameters.Add("mod", OracleDbType.Varchar2).Value = prog.Modalidad;
                cmd.Parameters.Add("idUni", OracleDbType.Decimal).Value = prog.IdUniversidad;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarPrograma(ProgramaViewModel prog)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = @"UPDATE programas p 
                       SET p.nombre = :nom, 
                           p.descripcion = :desc, 
                           p.area = :area, 
                           p.tipo_programa = :tipo, 
                           p.modalidad = :mod,
                           p.ref_universidad = (SELECT REF(u) FROM universidades u WHERE u.id_universidad = :idUni)
                       WHERE p.cod_programa = :cod";

                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("nom", OracleDbType.Varchar2).Value = prog.Nombre;
                cmd.Parameters.Add("desc", OracleDbType.Varchar2).Value = prog.Descripcion;
                cmd.Parameters.Add("area", OracleDbType.Varchar2).Value = prog.Area;
                cmd.Parameters.Add("tipo", OracleDbType.Varchar2).Value = prog.TipoPrograma;
                cmd.Parameters.Add("mod", OracleDbType.Varchar2).Value = prog.Modalidad;
                cmd.Parameters.Add("idUni", OracleDbType.Decimal).Value = prog.IdUniversidad;
                cmd.Parameters.Add("cod", OracleDbType.Varchar2).Value = prog.CodPrograma;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarPrograma(string cod)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = "DELETE FROM programas WHERE cod_programa = :cod";
                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("cod", OracleDbType.Varchar2).Value = cod;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<OfertaViewModel> ObtenerTodasOfertas()
        {
            List<OfertaViewModel> lista = new List<OfertaViewModel>();
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                // Navegamos por o.ref_programa para obtener los datos del programa
                string sql = @"SELECT o.id_oferta, o.fecha_inicio, o.fecha_fin, 
                              o.tipo_financiamiento, o.estado_oferta,
                              o.ref_programa.cod_programa AS cod_prog,
                              o.ref_programa.nombre AS nombre_prog
                       FROM ofertas o";

                OracleCommand cmd = new OracleCommand(sql, conn);
                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new OfertaViewModel
                        {
                            IdOferta = Convert.ToInt32(reader["id_oferta"]),
                            FechaInicio = Convert.ToDateTime(reader["fecha_inicio"]),
                            FechaFin = Convert.ToDateTime(reader["fecha_fin"]),
                            TipoFinanciamiento = reader["tipo_financiamiento"].ToString(),
                            EstadoOferta = reader["estado_oferta"].ToString(),
                            CodPrograma = reader["cod_prog"].ToString(),
                            NombrePrograma = reader["nombre_prog"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public OfertaViewModel ObtenerOfertaPorId(int id)
        {
            OfertaViewModel oferta = new OfertaViewModel();
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = @"SELECT o.id_oferta, o.fecha_inicio, o.fecha_fin, 
                              o.tipo_financiamiento, o.estado_oferta,
                              o.ref_programa.cod_programa AS cod_prog
                       FROM ofertas o WHERE o.id_oferta = :id";

                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("id", OracleDbType.Decimal).Value = id;
                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        oferta.IdOferta = Convert.ToInt32(reader["id_oferta"]);
                        oferta.FechaInicio = Convert.ToDateTime(reader["fecha_inicio"]);
                        oferta.FechaFin = Convert.ToDateTime(reader["fecha_fin"]);
                        oferta.TipoFinanciamiento = reader["tipo_financiamiento"].ToString();
                        oferta.EstadoOferta = reader["estado_oferta"].ToString();
                        oferta.CodPrograma = reader["cod_prog"].ToString();
                    }
                }
            }
            return oferta;
        }

        public void CrearOferta(OfertaViewModel of)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = @"INSERT INTO ofertas VALUES (
                        t_oferta(:id, 
                            (SELECT REF(p) FROM programas p WHERE p.cod_programa = :codProg), 
                            :f_ini, :f_fin, :finan, :est
                        )
                      )";

                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("id", OracleDbType.Decimal).Value = of.IdOferta;
                cmd.Parameters.Add("codProg", OracleDbType.Varchar2).Value = of.CodPrograma;
                cmd.Parameters.Add("f_ini", OracleDbType.Date).Value = of.FechaInicio;
                cmd.Parameters.Add("f_fin", OracleDbType.Date).Value = of.FechaFin;
                cmd.Parameters.Add("finan", OracleDbType.Varchar2).Value = of.TipoFinanciamiento;
                cmd.Parameters.Add("est", OracleDbType.Varchar2).Value = of.EstadoOferta;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarOferta(OfertaViewModel of)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = @"UPDATE ofertas o 
                       SET o.ref_programa = (SELECT REF(p) FROM programas p WHERE p.cod_programa = :codProg),
                           o.fecha_inicio = :f_ini, 
                           o.fecha_fin = :f_fin, 
                           o.tipo_financiamiento = :finan, 
                           o.estado_oferta = :est
                       WHERE o.id_oferta = :id";

                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("codProg", OracleDbType.Varchar2).Value = of.CodPrograma;
                cmd.Parameters.Add("f_ini", OracleDbType.Date).Value = of.FechaInicio;
                cmd.Parameters.Add("f_fin", OracleDbType.Date).Value = of.FechaFin;
                cmd.Parameters.Add("finan", OracleDbType.Varchar2).Value = of.TipoFinanciamiento;
                cmd.Parameters.Add("est", OracleDbType.Varchar2).Value = of.EstadoOferta;
                cmd.Parameters.Add("id", OracleDbType.Decimal).Value = of.IdOferta;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarOferta(int id)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = "DELETE FROM ofertas WHERE id_oferta = :id";
                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("id", OracleDbType.Decimal).Value = id;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Método para crear al postulante
        public void CrearPostulante(string doc, string nom, string ape, string correo, string nivel)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                using (OracleCommand cmd = new OracleCommand("sp_crear_postulante", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_doc", OracleDbType.Varchar2).Value = doc;
                    cmd.Parameters.Add("p_nombres", OracleDbType.Varchar2).Value = nom;
                    cmd.Parameters.Add("p_apellidos", OracleDbType.Varchar2).Value = ape;
                    cmd.Parameters.Add("p_correo", OracleDbType.Varchar2).Value = correo;
                    cmd.Parameters.Add("p_nivel", OracleDbType.Varchar2).Value = nivel;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Método para agregarle historial laboral
        public void AgregarExperiencia(string doc, string empresa, string cargo, DateTime inicio, DateTime? fin)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                using (OracleCommand cmd = new OracleCommand("sp_agregar_experiencia", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_doc", OracleDbType.Varchar2).Value = doc;
                    cmd.Parameters.Add("p_empresa", OracleDbType.Varchar2).Value = empresa;
                    cmd.Parameters.Add("p_cargo", OracleDbType.Varchar2).Value = cargo;
                    cmd.Parameters.Add("p_inicio", OracleDbType.Date).Value = inicio;

                    // Validamos si la fecha fin es nula (el usuario sigue trabajando ahí)
                    if (fin.HasValue)
                        cmd.Parameters.Add("p_fin", OracleDbType.Date).Value = fin.Value;
                    else
                        cmd.Parameters.Add("p_fin", OracleDbType.Date).Value = DBNull.Value;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<PostulanteViewModel> ObtenerPostulantes()
        {
            List<PostulanteViewModel> lista = new List<PostulanteViewModel>();
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = "SELECT doc_identidad, nombres, apellidos, correo, nivel_formacion FROM postulantes";
                OracleCommand cmd = new OracleCommand(sql, conn);
                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new PostulanteViewModel
                        {
                            DocIdentidad = reader["doc_identidad"].ToString(),
                            Nombres = reader["nombres"].ToString(),
                            Apellidos = reader["apellidos"].ToString(),
                            Correo = reader["correo"].ToString(),
                            NivelFormacion = reader["nivel_formacion"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public List<SedeViewModel> ObtenerSedes()
        {
            List<SedeViewModel> lista = new List<SedeViewModel>();
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = "SELECT * FROM sedes";
                OracleCommand cmd = new OracleCommand(sql, conn);
                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new SedeViewModel
                        {
                            CodSede = reader["cod_sede"].ToString(),
                            Nombre = reader["nombre"].ToString(),
                            TipoSede = reader["tipo_sede"].ToString(),
                            Responsable = reader["responsable"].ToString(),
                            Pais = reader["pais"].ToString(),
                            Departamento = reader["departamento"].ToString(),
                            Municipio = reader["municipio"].ToString(),
                            Direccion = reader["direccion"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public SedeViewModel ObtenerSedePorCod(string cod)
        {
            SedeViewModel sede = new SedeViewModel();
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = "SELECT * FROM sedes WHERE cod_sede = :cod";
                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("cod", OracleDbType.Varchar2).Value = cod;
                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        sede.CodSede = reader["cod_sede"].ToString();
                        sede.Nombre = reader["nombre"].ToString();
                        sede.TipoSede = reader["tipo_sede"].ToString();
                        sede.Responsable = reader["responsable"].ToString();
                        sede.Pais = reader["pais"].ToString();
                        sede.Departamento = reader["departamento"].ToString();
                        sede.Municipio = reader["municipio"].ToString();
                        sede.Direccion = reader["direccion"].ToString();
                    }
                }
            }
            return sede;
        }

        public void CrearSede(SedeViewModel s)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = @"INSERT INTO sedes VALUES (
                        t_sede(:cod, :nom, :tipo, :resp, :pais, :dep, :mun, :dir)
                      )";
                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("cod", OracleDbType.Varchar2).Value = s.CodSede;
                cmd.Parameters.Add("nom", OracleDbType.Varchar2).Value = s.Nombre;
                cmd.Parameters.Add("tipo", OracleDbType.Varchar2).Value = s.TipoSede;
                cmd.Parameters.Add("resp", OracleDbType.Varchar2).Value = s.Responsable;
                cmd.Parameters.Add("pais", OracleDbType.Varchar2).Value = s.Pais;
                cmd.Parameters.Add("dep", OracleDbType.Varchar2).Value = s.Departamento;
                cmd.Parameters.Add("mun", OracleDbType.Varchar2).Value = s.Municipio;
                cmd.Parameters.Add("dir", OracleDbType.Varchar2).Value = s.Direccion;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarSede(SedeViewModel s)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = @"UPDATE sedes 
                       SET nombre = :nom, tipo_sede = :tipo, responsable = :resp, 
                           pais = :pais, departamento = :dep, municipio = :mun, direccion = :dir 
                       WHERE cod_sede = :cod";
                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("nom", OracleDbType.Varchar2).Value = s.Nombre;
                cmd.Parameters.Add("tipo", OracleDbType.Varchar2).Value = s.TipoSede;
                cmd.Parameters.Add("resp", OracleDbType.Varchar2).Value = s.Responsable;
                cmd.Parameters.Add("pais", OracleDbType.Varchar2).Value = s.Pais;
                cmd.Parameters.Add("dep", OracleDbType.Varchar2).Value = s.Departamento;
                cmd.Parameters.Add("mun", OracleDbType.Varchar2).Value = s.Municipio;
                cmd.Parameters.Add("dir", OracleDbType.Varchar2).Value = s.Direccion;
                cmd.Parameters.Add("cod", OracleDbType.Varchar2).Value = s.CodSede;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarSede(string cod)
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                string sql = "DELETE FROM sedes WHERE cod_sede = :cod";
                OracleCommand cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("cod", OracleDbType.Varchar2).Value = cod;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
