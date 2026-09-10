using Microsoft.AspNetCore.Mvc;
using SistemaBecasWeb.Models;
using SistemaBecasWeb.Repositories;

namespace SistemaBecasWeb.Controllers
{
    public class ProgramaController : Controller
    {
        private readonly ISolicitudRepository _repository;

        public ProgramaController(ISolicitudRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            return View(_repository.ObtenerProgramas());
        }

        [HttpGet]
        public IActionResult Crear()
        {
            // Mandamos la lista de universidades para el ComboBox
            ViewBag.Universidades = _repository.ObtenerUniversidades();
            return View(new ProgramaViewModel());
        }

        [HttpPost]
        public IActionResult Crear(ProgramaViewModel prog)
        {
            _repository.CrearPrograma(prog);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Editar(string id)
        {
            ViewBag.Universidades = _repository.ObtenerUniversidades();
            var prog = _repository.ObtenerProgramaPorCod(id);
            return View(prog);
        }

        [HttpPost]
        public IActionResult Editar(ProgramaViewModel prog)
        {
            _repository.ActualizarPrograma(prog);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(string id)
        {
            _repository.EliminarPrograma(id);
            return RedirectToAction("Index");
        }
    }
}
