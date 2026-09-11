using Microsoft.AspNetCore.Mvc;
using SistemaBecasWeb.Repositories;
using SistemaBecasWeb.Filters;
using Oracle.ManagedDataAccess.Client;

namespace SistemaBecasWeb.Controllers
{
    public class SolicitudController : Controller
    {
        private readonly ISolicitudRepository _repository;

        public SolicitudController(ISolicitudRepository repository)
        {
            _repository = repository;
        }

        // GET: Carga el formulario inicial. Si viene idOferta (desde la portada), se preselecciona.
        [HttpGet]
        public IActionResult Index(int? idOferta)
        {
            ViewBag.Ofertas = _repository.ObtenerOfertasVigentes();
            ViewBag.OfertaSeleccionada = idOferta;
            return View();
        }

        // POST: Procesa los datos del formulario web
        [HttpPost]
        public IActionResult EnviarPostulacion(string docIdentidad, int idOferta, string resumen)
        {
            try
            {
                string mensajeBD = _repository.RegistrarSolicitud(docIdentidad, idOferta, resumen);
                TempData["Exito"] = mensajeBD;
                return RedirectToAction("Index");
            }
            catch (OracleException ex)
            {
                // Captura el límite de 3 postulaciones (RAISE_APPLICATION_ERROR -20001)
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Botón rápido para revisar aceptados
        [HttpPost]
        public IActionResult RevisarCupos(int idOfertaCupo)
        {
            try
            {
                int total = _repository.ContarAceptados(idOfertaCupo);
                TempData["Exito"] = $"Actualmente hay {total} estudiante(s) aceptado(s) en esta oferta.";
            }
            catch (OracleException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        // GET: Panel administrativo - lista todas las solicitudes (Pendiente/Aceptada/Rechazada)
        [HttpGet]
        [RequiereModoAdmin]
        public IActionResult Administrar()
        {
            var solicitudes = _repository.ObtenerSolicitudes();
            return View(solicitudes);
        }

        // POST: Acepta una solicitud puntual (llama a fn_aceptar_solicitud)
        [HttpPost]
        [RequiereModoAdmin]
        public IActionResult Aceptar(int idSolicitud)
        {
            try
            {
                string mensajeBD = _repository.AceptarSolicitud(idSolicitud);
                TempData["Exito"] = mensajeBD;
            }
            catch (OracleException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Administrar");
        }
    }
}
