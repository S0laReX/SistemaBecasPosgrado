using System.ComponentModel.DataAnnotations;

namespace SistemaBecasWeb.Models;

public class TransferenciaPostulacionViewModel
{
    [Required(ErrorMessage = "Ingrese el ID de la solicitud.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID de la solicitud debe ser positivo.")]
    public int? SolicitudId { get; set; }

    [Required(ErrorMessage = "Ingrese el ID de la nueva convocatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID de la convocatoria debe ser positivo.")]
    public int? OfertaDestinoId { get; set; }
}
