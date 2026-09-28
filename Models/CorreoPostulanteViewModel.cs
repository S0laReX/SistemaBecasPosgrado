using System.ComponentModel.DataAnnotations;

namespace SistemaBecasWeb.Models;

public class CorreoPostulanteViewModel
{
    [Required(ErrorMessage = "Ingrese el documento de identidad.")]
    [StringLength(20, ErrorMessage = "El documento admite hasta 20 caracteres.")]
    public string Documento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese el nuevo correo electrónico.")]
    [StringLength(100, ErrorMessage = "El correo admite hasta 100 caracteres.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    public string CorreoNuevo { get; set; } = string.Empty;
}

public class ProyectoEquipoViewModel
{
    public CorreoPostulanteViewModel Correo { get; set; } = new();
    public TransferenciaPostulacionViewModel Transferencia { get; set; } = new();
    public ReprogramarConvocatoriaViewModel Reprogramacion { get; set; } = new();
    public bool ProcedimientoDisponible { get; set; }
    public string? MensajeInstalacion { get; set; }
    public bool TransferenciaDisponible { get; set; }
    public string? MensajeTransferencia { get; set; }
    public bool ReprogramacionDisponible { get; set; }
    public string? MensajeReprogramacion { get; set; }
}
