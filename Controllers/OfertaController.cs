using Microsoft.AspNetCore.Mvc;
using SistemaBecasWeb.Models;
using SistemaBecasWeb.Repositories;
using SistemaBecasWeb.Filters;

namespace SistemaBecasWeb.Controllers
{
    [RequiereModoAdmin]
    public class OfertaController : Controller
    {
        private readonly ISolicitudRepository _repository;

        public OfertaController(ISolicitudRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            return View(_repository.ObtenerTodasOfertas());
        }

        [HttpGet]
        public IActionResult Crear()
        {
            // Necesitamos la lista de programas para que el usuario seleccione a cuál pertenece la oferta
            ViewBag.Programas = _repository.ObtenerProgramas();

            // Inicializamos fechas por defecto para que los controles HTML funcionen bien
            return View(new OfertaViewModel
            {
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddYears(1)
            });
        }

        [HttpPost]
        public IActionResult Crear(OfertaViewModel oferta)
        {
            _repository.CrearOferta(oferta);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            ViewBag.Programas = _repository.ObtenerProgramas();
            var oferta = _repository.ObtenerOfertaPorId(id);
            return View(oferta);
        }

        [HttpPost]
        public IActionResult Editar(OfertaViewModel oferta)
        {
            _repository.ActualizarOferta(oferta);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            _repository.EliminarOferta(id);
            return RedirectToAction("Index");
        }
    }
}
