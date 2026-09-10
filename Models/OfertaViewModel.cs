namespace SistemaBecasWeb.Models
{
    public class OfertaViewModel
    {
        public int IdOferta { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string TipoFinanciamiento { get; set; }
        public string EstadoOferta { get; set; } // 'Vigente' o 'Cerrada'

        // Datos del Programa Relacionado (REF)
        public string CodPrograma { get; set; }
        public string NombrePrograma { get; set; }
    }
}
