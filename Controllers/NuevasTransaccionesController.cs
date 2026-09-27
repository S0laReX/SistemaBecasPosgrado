using Microsoft.AspNetCore.Mvc;

namespace SistemaBecasWeb.Controllers;

public class NuevasTransaccionesController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
