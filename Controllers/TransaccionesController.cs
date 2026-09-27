using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using SistemaBecasWeb.Models;
using SistemaBecasWeb.Repositories;

namespace SistemaBecasWeb.Controllers;

public class TransaccionesController(TransaccionRepository repository, ILogger<TransaccionesController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index(string operacion = "candidato") => Mostrar(new() { Operacion = operacion });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Ejecutar([Bind(Prefix = "Formulario")] TransaccionViewModel formulario)
    {
        if (!ModelState.IsValid) return Mostrar(formulario);
        try
        {
            repository.Ejecutar(formulario);
            TempData["TransaccionExito"] = "Operación completada. Los cambios se guardaron en Oracle.";
            return RedirectToAction(nameof(Index), new { operacion = formulario.Operacion });
        }
        catch (OracleException ex)
        {
            logger.LogWarning("Operación {Operacion} rechazada por Oracle: {Codigo}", formulario.Operacion, ex.Number);
            var mensaje = ex.Number is >= 20001 and <= 20014
                ? ex.Message.Split('\n')[0].Split(": ", 2).Last()
                : "No se pudo guardar la operación. Compruebe la conexión, la instalación y los datos; si otro usuario está editando, vuelva a intentar.";
            ModelState.AddModelError("", mensaje);
            return Mostrar(formulario);
        }
        catch (InvalidOperationException)
        {
            ModelState.AddModelError("", "La conexión con Oracle no está configurada o no está disponible.");
            return Mostrar(formulario);
        }
    }

    private IActionResult Mostrar(TransaccionViewModel formulario)
    {
        PanelBecasViewModel panel;
        try { panel = repository.Consultar(); }
        catch (OracleException ex)
        {
            logger.LogWarning("Panel Oracle no disponible: {Codigo}", ex.Number);
            panel = new();
        }
        catch (InvalidOperationException) { panel = new(); }
        panel.Formulario = formulario;
        return View("Index", panel);
    }
}
