namespace SistemaBecasWeb.Models
{
    // Representa una fila del panel "Administrar Solicitudes".
    // Junta datos del postulante y de la oferta (navegando los REF) para no
    // tener que hacer varias consultas desde la vista.
    public class SolicitudViewModel
    {
        public int IdSolicitud { get; set; }

        // Datos del postulante (vía ref_postulante)
        public string DocIdentidad { get; set; }
        public string NombreCompleto { get; set; }

        // Datos de la oferta (vía ref_oferta -> ref_programa)
        public int IdOferta { get; set; }
        public string NombrePrograma { get; set; }

        // Resumen recortado (el campo real es un CLOB en la BD)
        public string ResumenCorto { get; set; }

        public string Estado { get; set; } // 'Pendiente', 'Aceptada', 'Rechazada'
    }
}
