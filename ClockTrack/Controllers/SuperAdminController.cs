using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClockTrack.Data;
using ClockTrack.Models;

namespace ClockTrack.Controllers
{
    public class SuperAdminController : Controller
    {
        private readonly ClockTrackContext _context;
        private readonly IConfiguration _config;

        public SuperAdminController(ClockTrackContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string password)
        {
            var masterPassword = _config["SuperAdmin:Password"];
            if (password == masterPassword)
            {
                HttpContext.Session.SetString("superadmin", "true");
                return RedirectToAction("Index");
            }
            TempData["Erro"] = "Senha incorreta.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("superadmin");
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("superadmin") != "true")
                return RedirectToAction("Login");

            var tenants = await _context.Tenants.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return View(tenants);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarLimites(int id, int maxUsers)
        {
            if (HttpContext.Session.GetString("superadmin") != "true")
                return RedirectToAction("Login");

            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant != null)
            {
                tenant.MaxUsers = maxUsers;
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = $"Limite de {tenant.Name} atualizado para {maxUsers} usuários.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ativar(int id)
        {
            if (HttpContext.Session.GetString("superadmin") != "true")
                return RedirectToAction("Login");

            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant != null)
            {
                tenant.IsActive = true;
                tenant.SubscriptionExpiresAt = DateTime.UtcNow.AddDays(30);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = $"{tenant.Name} ativado por 30 dias.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Bloquear(int id)
        {
            if (HttpContext.Session.GetString("superadmin") != "true")
                return RedirectToAction("Login");

            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant != null)
            {
                tenant.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = $"{tenant.Name} bloqueado.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EstenderTrial(int id, int dias = 14)
        {
            if (HttpContext.Session.GetString("superadmin") != "true")
                return RedirectToAction("Login");

            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant != null)
            {
                var base_ = tenant.TrialEndsAt.HasValue && tenant.TrialEndsAt > DateTime.UtcNow
                    ? tenant.TrialEndsAt.Value
                    : DateTime.UtcNow;
                tenant.TrialEndsAt = base_.AddDays(dias);
                tenant.IsActive = true;
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = $"Trial de {tenant.Name} estendido por {dias} dias.";
            }
            return RedirectToAction("Index");
        }
    }
}
