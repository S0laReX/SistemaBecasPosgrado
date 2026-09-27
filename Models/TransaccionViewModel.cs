using System.ComponentModel.DataAnnotations;

namespace SistemaBecasWeb.Models;

public class TransaccionViewModel : IValidatableObject
{
    [Required] public string Operacion { get; set; } = "candidato";
    [StringLength(20)] public string? Documento { get; set; }
    [StringLength(100)] public string? Nombres { get; set; }
    [StringLength(100)] public string? Apellidos { get; set; }
    [StringLength(100), EmailAddress] public string? Correo { get; set; }
    [StringLength(50)] public string? Nivel { get; set; }
    [StringLength(100)] public string? Empresa { get; set; }
    [StringLength(100)] public string? Cargo { get; set; }
    public DateTime? Inicio { get; set; }
    public DateTime? Fin { get; set; }
    public int? Oferta { get; set; }
    public int? Solicitud { get; set; }
    [StringLength(2000)] public string? Resumen { get; set; }
    public string? Estado { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var campos = Operacion switch {
            "candidato" => new[] { Documento, Nombres, Apellidos, Correo, Nivel },
            "experiencia" => new[] { Documento, Empresa, Cargo },
            "postular" => new[] { Documento, Resumen },
            "resolver" or "cerrar" => Array.Empty<string?>(),
            _ => new string?[] { null }
        };
        if (campos.Any(string.IsNullOrWhiteSpace)) yield return new("Complete todos los campos obligatorios de la operación.");
        if (Operacion == "experiencia" && (Inicio is null || Fin < Inicio)) yield return new("Revise las fechas de experiencia.");
        if ((Operacion is "postular" or "cerrar") && !(Oferta > 0)) yield return new("Seleccione una oferta.");
        if (Operacion == "resolver" && (!(Solicitud > 0) || Estado is not ("Aceptada" or "Rechazada"))) yield return new("Seleccione una solicitud y una resolución válida.");
    }
}

public record OpcionBeca(int Id, string Descripcion);
public class PanelBecasViewModel
{
    public TransaccionViewModel Formulario { get; set; } = new();
    public List<OpcionBeca> Ofertas { get; set; } = [];
    public List<OpcionBeca> Solicitudes { get; set; } = [];
    public bool Disponible { get; set; }
}
