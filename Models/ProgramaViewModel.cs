namespace SistemaBecasWeb.Models
{
    public class ProgramaViewModel
    {
        public string CodPrograma { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Area { get; set; }
        public string TipoPrograma { get; set; }
        public string Modalidad { get; set; }

        // Datos de la Universidad relacionada (Foránea / REF)
        public int IdUniversidad { get; set; }
        public string NombreUniversidad { get; set; }
    }
}
