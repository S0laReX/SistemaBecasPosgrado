using Microsoft.AspNetCore.Mvc;
using SistemaBecasWeb.Repositories;
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

        // GET: Carga el formulario inicial
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Ofertas = _repository.ObtenerOfertasVigentes();
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
    }
}
