namespace SistemaBecasWeb.Models
{
    public class ExperienciaViewModel
    {
        public string DocIdentidad { get; set; } // Para saber a qué postulante le estamos agregando la experiencia
        public string Empresa { get; set; }
        public string Cargo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; } // Nullable (?) por si el usuario sigue trabajando ahí
    }
}

