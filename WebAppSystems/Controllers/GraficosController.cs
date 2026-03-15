using Microsoft.AspNetCore.Mvc;
using WebAppSystems.Services;
using static WebAppSystems.Helper.Sessao;

namespace WebAppSystems.Controllers
{
    public class GraficosController : Controller
    {

        private readonly ProcessRecordsService _processRecordsService;

        public GraficosController(ProcessRecordsService processRecordsService)
        {
            _processRecordsService = processRecordsService;
        }
        public IActionResult Index()
        {
            try
            {
                var chartData = _processRecordsService.GetChartData();
                return View(chartData);
            }
            catch (SessionExpiredException)
            {
                // Redirecione para a página de login se a sessão expirou
                TempData["MensagemAviso"] = "A sessão expirou. Por favor, faça login novamente.";
                return RedirectToAction("Index", "Login");
            }
        }
    }
}
