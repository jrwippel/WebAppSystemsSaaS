using Microsoft.AspNetCore.Mvc;
using ClockTrack.Filters;

namespace ClockTrack.Controllers
{
    [PaginaParaUsuarioLogado]
    public class RestritoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
