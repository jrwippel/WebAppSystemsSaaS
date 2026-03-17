using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;
using ClockTrack.Data;
using ClockTrack.Models;
using ClockTrack.Services;

namespace ClockTrack.Controllers
{
    public class AssinaturaController : Controller
    {
        private readonly ClockTrackContext _context;
        private readonly MercadoPagoService _mp;
        private readonly ILogger<AssinaturaController> _logger;

        public AssinaturaController(ClockTrackContext context, MercadoPagoService mp, ILogger<AssinaturaController> logger)
        {
            _context = context;
            _mp = mp;
            _logger = logger;
        }

        // Tela de planos (acessível logado ou não)
        public IActionResult Planos()
        {
            ViewBag.Planos = _mp.ObterPlanos();
            return View();
        }

        // Inicia o checkout no Mercado Pago
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assinar(string plano)
        {
            var sessaoJson = HttpContext.Session.GetString("sessaoUsuarioLogado");
            if (string.IsNullOrEmpty(sessaoJson))
                return RedirectToAction("Index", "Login");

            var attorney = JsonConvert.DeserializeObject<Attorney>(sessaoJson);
            if (attorney == null)
                return RedirectToAction("Index", "Login");

            var tenant = await _context.Tenants.FindAsync(attorney.TenantId);
            if (tenant == null)
                return RedirectToAction("Index", "Login");

            try
            {
                var resultado = await _mp.CriarPreferenciaAsync(
                    tenantId: tenant.Id,
                    plano: plano,
                    emailPagador: attorney.Email,
                    nomePagador: attorney.Name
                );

                // Em sandbox usa SandboxInitPoint, em produção usa InitPoint
                var url = resultado.InitPoint;
                return Redirect(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar preferencia MP");
                TempData["Erro"] = "Erro ao iniciar pagamento. Tente novamente.";
                return RedirectToAction("Planos");
            }
        }

        // Retorno após pagamento aprovado
        public async Task<IActionResult> Sucesso(int tenantId, string plano, string payment_id, string status)
        {
            if (status == "approved" && tenantId > 0)
            {
                await AtivarAssinatura(tenantId, plano, payment_id);
            }

            ViewBag.Plano = plano;
            ViewBag.Status = status;
            return View();
        }

        public IActionResult Falha()
        {
            return View();
        }

        public IActionResult Pendente()
        {
            return View();
        }

        // Webhook do Mercado Pago
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Webhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                _logger.LogInformation("MP Webhook: {body}", body);

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                var type = root.TryGetProperty("type", out var t) ? t.GetString() : "";
                if (type != "payment") return Ok();

                var paymentId = root.GetProperty("data").GetProperty("id").GetString();
                if (string.IsNullOrEmpty(paymentId)) return Ok();

                var payment = await _mp.BuscarPagamentoAsync(paymentId);
                if (payment == null || payment.Status != "approved") return Ok();

                // external_reference = "tenantId|plano"
                var parts = payment.ExternalReference.Split('|');
                if (parts.Length < 2) return Ok();

                if (int.TryParse(parts[0], out int tenantId))
                    await AtivarAssinatura(tenantId, parts[1], paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no webhook MP");
            }

            return Ok();
        }

        // Tela de assinatura expirada
        public IActionResult Expirado()
        {
            var sessaoJson = HttpContext.Session.GetString("sessaoUsuarioLogado");
            if (string.IsNullOrEmpty(sessaoJson))
                return RedirectToAction("Index", "Login");

            var attorney = JsonConvert.DeserializeObject<Attorney>(sessaoJson);
            if (attorney == null)
                return RedirectToAction("Index", "Login");

            var tenant = _context.Tenants.Find(attorney.TenantId);
            if (tenant == null)
                return RedirectToAction("Index", "Login");

            if (!tenant.IsBlocked)
                return RedirectToAction("Index", "Home");

            ViewBag.TenantName = tenant.Name;
            ViewBag.TrialEndsAt = tenant.TrialEndsAt;
            ViewBag.IsTrialExpired = tenant.IsTrialExpired;
            ViewBag.Planos = _mp.ObterPlanos();
            return View();
        }

        private async Task AtivarAssinatura(int tenantId, string plano, string paymentId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null) return;

            var planos = _mp.ObterPlanos();
            if (!planos.TryGetValue(plano, out var info)) return;

            tenant.IsActive = true;
            tenant.PlanoAtual = plano;
            tenant.MaxUsers = info.MaxUsers;
            tenant.SubscriptionExpiresAt = DateTime.UtcNow.AddDays(info.DiasAssinatura);
            tenant.MercadoPagoSubscriptionId = paymentId;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Tenant {id} ativado com plano {plano}", tenantId, plano);
        }
    }
}
