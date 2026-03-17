using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ClockTrack.Data;
using ClockTrack.Models;

namespace ClockTrack.Controllers
{
    public class AssinaturaController : Controller
    {
        private readonly ClockTrackContext _context;

        public AssinaturaController(ClockTrackContext context)
        {
            _context = context;
        }

        public IActionResult Expirado()
        {
            string sessaoUsuario = HttpContext.Session.GetString("sessaoUsuarioLogado");
            if (string.IsNullOrEmpty(sessaoUsuario))
                return RedirectToAction("Index", "Login");

            var attorney = JsonConvert.DeserializeObject<Attorney>(sessaoUsuario);
            if (attorney == null)
                return RedirectToAction("Index", "Login");

            var tenant = _context.Tenants.Find(attorney.TenantId);
            if (tenant == null)
                return RedirectToAction("Index", "Login");

            // Se não está mais bloqueado (admin ativou), redireciona para home
            if (!tenant.IsBlocked)
                return RedirectToAction("Index", "Home");

            ViewBag.TenantName = tenant.Name;
            ViewBag.TrialEndsAt = tenant.TrialEndsAt;
            ViewBag.IsTrialExpired = tenant.IsTrialExpired;
            return View();
        }
    }
}
