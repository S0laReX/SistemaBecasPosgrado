using System.ComponentModel.DataAnnotations;

namespace SistemaBecasWeb.Models;

public class ReprogramarConvocatoriaViewModel
{
    [Required(ErrorMessage = "Ingrese el ID de la convocatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID de la convocatoria debe ser positivo.")]
    public int? OfertaId { get; set; }

    [Required(ErrorMessage = "Ingrese la nueva fecha de inicio.")]
    [DataType(DataType.Date)]
    public DateTime? NuevaFechaInicio { get; set; }

    [Required(ErrorMessage = "Ingrese la nueva fecha de cierre.")]
    [DataType(DataType.Date)]
    public DateTime? NuevaFechaFin { get; set; }
}