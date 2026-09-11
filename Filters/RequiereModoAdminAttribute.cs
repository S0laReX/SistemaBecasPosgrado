using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SistemaBecasWeb.Filters
{
    // NOTA: Este proyecto no tiene un sistema de login/roles real (no hay Identity).
    // Este filtro es una separación a nivel de interfaz: usa una cookie ("modo_vista")
    // que el usuario cambia con el botón de la barra de navegación. Evita que alguien
    // "se pierda" en pantallas administrativas estando en modo Postulante, pero no debe
    // considerarse una medida de seguridad/autenticación.
    public static class ModoVista
    {
        public const string NombreCookie = "modo_vista";
        public const string Admin = "admin";
        public const string Postulante = "postulante";

        public static string ObtenerModo(HttpRequest request)
        {
            var valor = request.Cookies[NombreCookie];
            return valor == Admin ? Admin : Postulante;
        }
    }

    public class RequiereModoAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var modo = ModoVista.ObtenerModo(context.HttpContext.Request);

            if (modo != ModoVista.Admin)
            {
                // Guardamos un aviso en TempData y devolvemos al portal público
                var tempDataFactory = context.HttpContext.RequestServices.GetService(typeof(ITempDataDictionaryFactory)) as ITempDataDictionaryFactory;
                if (tempDataFactory != null)
                {
                    var tempData = tempDataFactory.GetTempData(context.HttpContext);
                    tempData["Error"] = "Esa sección es solo para el modo Administrador. Cambia de vista con el botón de la barra superior.";
                    tempData.Save();
                }

                context.Result = new RedirectToActionResult("Index", "Home", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
