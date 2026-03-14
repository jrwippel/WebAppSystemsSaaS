using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppSystems.Data;
using WebAppSystems.Helper;
using WebAppSystems.Models;
using WebAppSystems.Models.Enums;
using System.Linq;

namespace WebAppSystems.Controllers
{
    public class RegistroController : Controller
    {
        private readonly WebAppSystemsContext _context;

        public RegistroController(WebAppSystemsContext context)
        {
            _context = context;
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
                    MaxUsers = 10,      // Plano inicial: 10 usuários
                    MaxClients = 100,   // Plano inicial: 100 clientes
                    MaxStorageMB = 2048 // Plano inicial: 2GB
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
    }
}
