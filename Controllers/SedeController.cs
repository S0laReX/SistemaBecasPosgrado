using Microsoft.AspNetCore.Mvc;
using SistemaBecasWeb.Models;
using SistemaBecasWeb.Repositories;

namespace SistemaBecasWeb.Controllers
{
    public class SedeController : Controller
    {
        private readonly ISolicitudRepository _repository;

        public SedeController(ISolicitudRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            return View(_repository.ObtenerSedes());
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new SedeViewModel());
        }

        [HttpPost]
        public IActionResult Crear(SedeViewModel sede)
        {
            _repository.CrearSede(sede);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Editar(string id)
        {
            var sede = _repository.ObtenerSedePorCod(id);
            return View(sede);
        }

        [HttpPost]
        public IActionResult Editar(SedeViewModel sede)
        {
            _repository.ActualizarSede(sede);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(string id)
        {
            _repository.EliminarSede(id);
            return RedirectToAction("Index");
        }
    }
}
