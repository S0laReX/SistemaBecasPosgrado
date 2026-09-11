using Microsoft.AspNetCore.Mvc;
using SistemaBecasWeb.Models;
using SistemaBecasWeb.Repositories;
using SistemaBecasWeb.Filters;
using System.Diagnostics;

namespace SistemaBecasWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISolicitudRepository _repository;

        public HomeController(ISolicitudRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            // Portal comercial: mostramos las ofertas vigentes como vitrina de programas
            var ofertas = _repository.ObtenerOfertasVigentesDetalle();
            return View(ofertas);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Cambia entre "modo Postulante" (portal público) y "modo Administrador"
        // guardando la elección en una cookie que lee el layout y el filtro RequiereModoAdmin.
        [HttpGet]
        public IActionResult CambiarModo(string modo, string returnUrl)
        {
            var modoFinal = modo == ModoVista.Admin ? ModoVista.Admin : ModoVista.Postulante;

            Response.Cookies.Append(ModoVista.NombreCookie, modoFinal, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddHours(8),
                IsEssential = true,
                HttpOnly = false
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
