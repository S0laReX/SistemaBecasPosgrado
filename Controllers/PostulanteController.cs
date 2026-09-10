using Microsoft.AspNetCore.Mvc;
using SistemaBecasWeb.Models;
using SistemaBecasWeb.Repositories;

namespace SistemaBecasWeb.Controllers
{
    public class PostulanteController : Controller
    {
        private readonly ISolicitudRepository _repository;

        public PostulanteController(ISolicitudRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var postulantes = _repository.ObtenerPostulantes();
            return View(postulantes);
        }

        // PASO 1: Muestra el formulario de datos personales
        [HttpGet]
        public IActionResult Crear()
        {
            return View(new PostulanteViewModel());
        }

        // PASO 1: Recibe los datos personales, inicializa la tabla anidada vacía y redirige
        [HttpPost]
        public IActionResult Crear(PostulanteViewModel post)
        {
            _repository.CrearPostulante(post.DocIdentidad, post.Nombres, post.Apellidos, post.Correo, post.NivelFormacion);

            // Pasamos el documento de identidad a la URL de la siguiente pantalla
            return RedirectToAction("Experiencias", new { doc = post.DocIdentidad });
        }

        // PASO 2: Pantalla para ir agregando empleos al postulante recién creado
        [HttpGet]
        public IActionResult Experiencias(string doc)
        {
            ViewBag.DocIdentidad = doc;
            return View(new ExperienciaViewModel
            {
                DocIdentidad = doc,
                FechaInicio = DateTime.Now
            });
        }

        // PASO 2: Guarda una experiencia dentro de la tabla anidada y recarga la misma página
        [HttpPost]
        public IActionResult AgregarExperiencia(ExperienciaViewModel exp)
        {
            _repository.AgregarExperiencia(exp.DocIdentidad, exp.Empresa, exp.Cargo, exp.FechaInicio, exp.FechaFin);

            // Usamos TempData para mostrar un mensaje de éxito sin perder la vista
            TempData["Exito"] = "Experiencia agregada correctamente. Puedes añadir otra o finalizar.";

            return RedirectToAction("Experiencias", new { doc = exp.DocIdentidad });
        }
    }
}
