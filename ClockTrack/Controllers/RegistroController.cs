using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClockTrack.Data;
using ClockTrack.Helper;
using ClockTrack.Models;
using ClockTrack.Models.Enums;
using System.Linq;

namespace ClockTrack.Controllers
{
    public class RegistroController : Controller
    {
        private readonly ClockTrackContext _context;
        private readonly IEmail _email;

        public RegistroController(ClockTrackContext context, IEmail email)
        {
            _context = context;
            _email = email;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(RegistroTenantModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Mostrar erros de validação específicos
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    if (errors.Any())
                    {
                        TempData["MensagemErro"] = string.Join(" | ", errors);
                    }
                    return View("Index", model);
                }

                // Verificar se o subdomínio já existe
                var subdomainExiste = await _context.Tenants
                    .AnyAsync(t => t.Subdomain.ToLower() == model.Subdomain.ToLower());

                if (subdomainExiste)
                {
                    TempData["MensagemErro"] = "Este subdomínio já está em uso. Por favor, escolha outro.";
                    return View("Index", model);
                }

                // Verificar se o email do admin já existe (em qualquer tenant)
                // Email deve ser único pois é pessoal
                var emailExiste = await _context.Attorney
                    .IgnoreQueryFilters()
                    .AnyAsync(a => a.Email.ToLower() == model.EmailAdmin.ToLower());

                if (emailExiste)
                {
                    TempData["MensagemErro"] = "Este email já está em uso. Por favor, use outro email.";
                    return View("Index", model);
                }

                // Criar o Tenant
                var tenant = new Tenant
                {
                    Name = model.NomeEmpresa,
                    Subdomain = model.Subdomain.ToLower(),
                    Document = model.Document ?? "",
                    Email = model.EmailEmpresa,
                    Phone = model.TelefoneEmpresa ?? "",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    TrialEndsAt = DateTime.UtcNow.AddDays(14), // 14 dias de trial
                    MaxUsers = 10,
                    MaxClients = 100,
                    MaxStorageMB = 2048
                };

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                // Criar departamento padrão
                var department = new Department("Administrativo")
                {
                    TenantId = tenant.Id
                };
                _context.Department.Add(department);
                await _context.SaveChangesAsync();

                // Criar usuário administrador
                var senhaHash = Criptografia.GerarHash(model.Senha);
                var admin = new Attorney(
                    name: model.NomeAdmin,
                    email: model.EmailAdmin,
                    phone: model.TelefoneAdmin ?? "",
                    birthDate: DateTime.Now,
                    department: department,
                    perfil: ProfileEnum.Admin,
                    password: senhaHash,
                    registerDate: DateTime.Now,
                    updateDate: DateTime.Now,
                    login: model.Login
                )
                {
                    TenantId = tenant.Id
                };

                _context.Attorney.Add(admin);
                await _context.SaveChangesAsync();

                // Enviar email de boas-vindas (sem bloquear o fluxo em caso de falha)
                _ = EnviarEmailBoasVindasAsync(model, tenant);

                TempData["MensagemSucesso"] = $"Conta criada com sucesso! Bem-vindo ao Time Tracker, {model.NomeEmpresa}!";
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar tenant: {ex.Message}");
                TempData["MensagemErro"] = "Erro ao criar conta. Tente novamente ou entre em contato com o suporte.";
                return View("Index", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerificarSubdominio(string subdomain)
        {
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                return Json(new { disponivel = false, mensagem = "Subdomínio inválido" });
            }

            var existe = await _context.Tenants
                .AnyAsync(t => t.Subdomain.ToLower() == subdomain.ToLower());

            return Json(new { 
                disponivel = !existe, 
                mensagem = existe ? "Subdomínio já está em uso" : "Subdomínio disponível" 
            });
        }

        private async Task EnviarEmailBoasVindasAsync(RegistroTenantModel model, Tenant tenant)
        {
            try
            {
                var trialFim = tenant.TrialEndsAt!.Value.ToString("dd/MM/yyyy");
                var html = $@"
<!DOCTYPE html>
<html lang='pt-br'>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;background:#f4f6fb;font-family:-apple-system,BlinkMacSystemFont,Segoe UI,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f6fb;padding:40px 0;'>
    <tr><td align='center'>
      <table width='560' cellpadding='0' cellspacing='0' style='background:white;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);'>
        <!-- Header -->
        <tr>
          <td style='background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);padding:36px 40px;text-align:center;'>
            <h1 style='color:white;margin:0;font-size:26px;font-weight:700;'>🕐 ClockTrack</h1>
            <p style='color:rgba(255,255,255,0.85);margin:8px 0 0;font-size:15px;'>Bem-vindo ao sistema!</p>
          </td>
        </tr>
        <!-- Body -->
        <tr>
          <td style='padding:36px 40px;'>
            <p style='font-size:16px;color:#2d3748;margin:0 0 16px;'>Olá, <strong>{model.NomeAdmin}</strong>!</p>
            <p style='font-size:14px;color:#4a5568;line-height:1.7;margin:0 0 24px;'>
              Sua conta foi criada com sucesso. Você tem <strong>14 dias de acesso gratuito</strong> para explorar todas as funcionalidades do sistema.
            </p>
            <!-- Info box -->
            <table width='100%' cellpadding='0' cellspacing='0' style='background:#f7f8fc;border-radius:10px;border:1px solid #e2e8f0;margin-bottom:24px;'>
              <tr>
                <td style='padding:20px 24px;'>
                  <p style='margin:0 0 8px;font-size:13px;color:#718096;'>Dados de acesso</p>
                  <p style='margin:0 0 6px;font-size:14px;color:#2d3748;'><strong>Empresa:</strong> {model.NomeEmpresa}</p>
                  <p style='margin:0 0 6px;font-size:14px;color:#2d3748;'><strong>Login:</strong> {model.Login}</p>
                  <p style='margin:0;font-size:14px;color:#2d3748;'><strong>Trial até:</strong> {trialFim}</p>
                </td>
              </tr>
            </table>
            <p style='font-size:14px;color:#4a5568;line-height:1.7;margin:0 0 24px;'>
              Após o período de teste, entre em contato para assinar e continuar usando sem interrupções.
            </p>
            <!-- Contact -->
            <table width='100%' cellpadding='0' cellspacing='0' style='background:#667eea;border-radius:10px;margin-bottom:8px;'>
              <tr>
                <td style='padding:16px 24px;text-align:center;'>
                  <p style='margin:0;color:white;font-size:13px;'>Dúvidas ou para assinar: <a href='mailto:jrwsolucoesti@hotmail.com' style='color:white;font-weight:700;'>jrwsolucoesti@hotmail.com</a> &nbsp;|&nbsp; <a href='https://wa.me/5547999346159' style='color:white;font-weight:700;'>+55 47 99934-6159</a></p>
                </td>
              </tr>
            </table>
          </td>
        </tr>
        <!-- Footer -->
        <tr>
          <td style='padding:16px 40px;border-top:1px solid #e2e8f0;text-align:center;'>
            <p style='margin:0;font-size:12px;color:#a0aec0;'>JRW Soluções em TI &mdash; Time Tracker Jurídico</p>
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";

                await _email.EnviarAsync(
                    email: model.EmailAdmin,
                    assunto: $"Bem-vindo ao Time Tracker — {model.NomeEmpresa}",
                    mensagem: $"Bem-vindo, {model.NomeAdmin}! Sua conta foi criada. Trial até {trialFim}.",
                    htmlBody: html
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar email de boas-vindas: {ex.Message}");
            }
        }
    }
}
