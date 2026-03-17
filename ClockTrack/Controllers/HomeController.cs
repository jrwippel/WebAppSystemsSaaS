using Microsoft.AspNetCore.Mvc;
using ClockTrack.Helper;
using ClockTrack.Models;
using ClockTrack.Services;
using static ClockTrack.Helper.Sessao;

namespace ClockTrack.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProcessRecordsService _processRecordsService;
        private readonly ISessao _isessao;

        public HomeController(ProcessRecordsService processRecordsService, ISessao isessao)
        {
            _processRecordsService = processRecordsService;
            _isessao = isessao;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                Attorney usuario = _isessao.BuscarSessaoDoUsuario();
                ViewBag.LoggedUserId = usuario.Id;
                ViewBag.CurrentUserPerfil = usuario.Perfil;

                var chartData = _processRecordsService.GetChartData();
                var kpis = await _processRecordsService.GetDashboardKpis(usuario.Id, usuario.Perfil);
                ViewBag.Kpis = kpis;
                return View(chartData);
            }
            catch (SessionExpiredException)
            {
                // Redirecione para a p�gina de login se a sess�o expirou
                TempData["MensagemAviso"] = "A sess�o expirou. Por favor, fa�a login novamente.";
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string type)
        {
            try
            {
                ChartData chartData;

                if (type == "cliente")
                {
                    chartData = _processRecordsService.GetChartData();
                }
                else if (type == "tipo")
                {
                    chartData = _processRecordsService.GetChartDataByActivityType();
                }
                else if (type == "area")
                {
                    chartData = _processRecordsService.GetChartDataByArea();
                }
                else if (type == "timeline")
                {
                    string period = Request.Query["period"].ToString();
                    if (string.IsNullOrEmpty(period)) period = "month";
                    chartData = _processRecordsService.GetChartDataByTimeline(period);
                }
                else
                {
                    return BadRequest("Tipo de gr�fico inv�lido.");
                }

                return Json(new
                {
                    labels = chartData.ClientNames,
                    values = chartData.ClientValues
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao gerar os dados do gr�fico.");
            }
        }





        public IActionResult About()
        {
            try
            {
                Attorney usuario = _isessao.BuscarSessaoDoUsuario();
                ViewBag.LoggedUserId = usuario.Id;
                ViewBag.CurrentUserPerfil = usuario.Perfil;
                return View();
            }
            catch (SessionExpiredException)
            {
                TempData["MensagemAviso"] = "A sess�o expirou. Por favor, fa�a login novamente.";
                return RedirectToAction("Index", "Login");
            }
        }

        [Route("Home/Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            ViewBag.StatusCode = statusCode;
            return View();
        }
    }
}