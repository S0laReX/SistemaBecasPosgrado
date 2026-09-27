using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using SistemaBecasWeb.Filters;
using SistemaBecasWeb.Models;
using SistemaBecasWeb.Repositories;

namespace SistemaBecasWeb.Controllers;

public class NuevasTransaccionesController(
    CorreoPostulanteRepository repository,
    ILogger<NuevasTransaccionesController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(CrearModelo(new()));

    [HttpPost, ValidateAntiForgeryToken, RequiereModoAdmin]
    public IActionResult ActualizarCorreo([Bind(Prefix = "Correo")] CorreoPostulanteViewModel correo)
    {
        if (!ModelState.IsValid) return View("Index", CrearModelo(correo));

        try
        {
            repository.Actualizar(correo.Documento, correo.CorreoNuevo);
            TempData["CorreoExito"] = "Correo actualizado correctamente en Oracle.";
            return RedirectToAction(nameof(Index), "NuevasTransacciones", null, "transaccion-uno");
        }
        catch (OracleException ex)
        {
            logger.LogWarning("Actualización de correo rechazada por Oracle: {Codigo}", ex.Number);
            var mensaje = ex.Number is >= 20101 and <= 20104
                ? ex.Message.Split('\n')[0].Split(": ", 2).Last()
                : ex.Number is 54 or 30006
                    ? "Otro usuario está modificando este postulante. Inténtelo nuevamente."
                    : "No se pudo guardar el correo. Verifique la conexión y que el procedimiento esté instalado.";
            ModelState.AddModelError(string.Empty, mensaje);
        }
        catch (InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, "Configure la conexión OracleDB antes de guardar.");
        }

        return View("Index", CrearModelo(correo));
    }

    private ProyectoEquipoViewModel CrearModelo(CorreoPostulanteViewModel correo)
    {
        var modelo = new ProyectoEquipoViewModel { Correo = correo };
        try
        {
            modelo.ProcedimientoDisponible = repository.EstaInstalado();
            if (!modelo.ProcedimientoDisponible)
                modelo.MensajeInstalacion = "Instale Database/04_actualizar_correo_postulante.sql en su esquema Oracle para activar esta operación.";
        }
        catch (OracleException ex)
        {
            logger.LogWarning("No se pudo verificar el procedimiento: {Codigo}", ex.Number);
            modelo.MensajeInstalacion = "No se pudo comprobar la conexión con Oracle.";
        }
        catch (InvalidOperationException)
        {
            modelo.MensajeInstalacion = "Configure la conexión OracleDB localmente para activar esta operación.";
        }

        return modelo;
    }
}
