using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebAppSystems.Data;
using WebAppSystems.Models;

namespace WebAppSystems.Controllers
{
    public class AssinaturaController : Controller
    {
        private readonly WebAppSystemsContext _context;

        public AssinaturaController(WebAppSystemsContext context)
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
