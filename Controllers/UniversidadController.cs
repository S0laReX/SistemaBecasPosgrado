using Microsoft.AspNetCore.Mvc;
using SistemaBecasWeb.Models;
using SistemaBecasWeb.Repositories;
using SistemaBecasWeb.Filters;

namespace SistemaBecasWeb.Controllers
{
    [RequiereModoAdmin]
    public class UniversidadController : Controller
    {
        private readonly ISolicitudRepository _repository;

        public UniversidadController(ISolicitudRepository repository)
        {
            _repository = repository;
        }

        // READ: Lista general
        public IActionResult Index()
        {
            return View(_repository.ObtenerUniversidades());
        }

        // CREATE: Muestra el formulario vacío
        [HttpGet]
        public IActionResult Crear()
        {
            return View(new UniversidadViewModel());
        }

        // CREATE: Recibe los datos y guarda
        [HttpPost]
        public IActionResult Crear(UniversidadViewModel uni)
        {
            _repository.CrearUniversidad(uni);
            return RedirectToAction("Index");
        }

        // UPDATE: Muestra el formulario con los datos cargados
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var uni = _repository.ObtenerUniversidadPorId(id);
            return View(uni);
        }

        // UPDATE: Recibe los datos modificados y guarda
        [HttpPost]
        public IActionResult Editar(UniversidadViewModel uni)
        {
            _repository.ActualizarUniversidad(uni);
            return RedirectToAction("Index");
        }

        // DELETE: Elimina directamente (en un sistema real se haría una pantalla de confirmación)
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            _repository.EliminarUniversidad(id);
            return RedirectToAction("Index");
        }
    }
}
