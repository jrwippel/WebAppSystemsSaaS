using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using ClockTrack.Filters;
using ClockTrack.Helper;
using ClockTrack.Models;
using ClockTrack.Models.Enums;
using ClockTrack.Models.ViewModels;
using ClockTrack.Services;
using ClockTrack.Services.Exceptions;
using static ClockTrack.Helper.Sessao;

namespace ClockTrack.Controllers
{
    [PaginaParaUsuarioLogado]
[PaginaRestritaSomenteAdmin]
    public class AttorneysController : Controller
    {
        private readonly AttorneyService _attorneyService;

        private readonly DepartmentService _departmentService;

        private readonly ISessao _isessao;
        private readonly ClockTrack.Data.ClockTrackContext _context;

        public AttorneysController(AttorneyService attorneyService, DepartmentService departmentService, ISessao isessao, ClockTrack.Data.ClockTrackContext context)
        {
            _attorneyService = attorneyService;
            _departmentService = departmentService;
            _isessao = isessao;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                Attorney usuario = _isessao.BuscarSessaoDoUsuario();
                ViewBag.LoggedUserId = usuario.Id;
                ViewBag.CurrentUserPerfil = usuario.Perfil;

                var list = await _attorneyService.FindAllAsync();
                return View(list);
            }
            catch (SessionExpiredException)
            {
                // Redirecione para a p�gina de login se a sess�o expirou
                TempData["MensagemAviso"] = "A sess�o expirou. Por favor, fa�a login novamente.";
                return RedirectToAction("Index", "Login");
            }
        }

       
        public async Task<IActionResult> Create()
        {
            Attorney usuario = _isessao.BuscarSessaoDoUsuario();
            ViewBag.CurrentUserPerfil = usuario.Perfil;
            var departments = await _departmentService.FindAllAsync();
            var viewModel = new AttorneyFormViewModel { Departments = departments };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Attorney attorney) 
        {

            Attorney usuario = _isessao.BuscarSessaoDoUsuario();
            ViewBag.LoggedUserId = usuario.Id;
            ViewBag.CurrentUserPerfil = usuario.Perfil;

            // Valida��o: somente Admin pode criar outro usu�rio com perfil de Admin
            if (usuario.Perfil != ProfileEnum.Admin && attorney.Perfil == ProfileEnum.Admin)
            {
                ModelState.AddModelError(string.Empty, "Voc� n�o tem permiss�o para criar um usu�rio com perfil de Administrador.");
                var departments = await _departmentService.FindAllAsync();
                var viewModel = new AttorneyFormViewModel { Attorney = attorney, Departments = departments };
                return View(viewModel);
            }

            // Validação: verificar se o email já existe (email deve ser único no sistema)
            var emailExiste = await _attorneyService.EmailExistsAsync(attorney.Email);
            if (emailExiste)
            {
                ModelState.AddModelError("Attorney.Email", "Este email já está em uso. Por favor, use outro email.");
                var departments = await _departmentService.FindAllAsync();
                var viewModel = new AttorneyFormViewModel { Attorney = attorney, Departments = departments };
                return View(viewModel);
            }

            // Validação: verificar limite de usuários do tenant
            var tenant = await _context.Tenants.FindAsync(usuario.TenantId);
            if (tenant != null)
            {
                var totalUsuarios = await _context.Attorney.IgnoreQueryFilters()
                    .CountAsync(a => a.TenantId == usuario.TenantId && !a.Inativo);
                if (totalUsuarios >= tenant.MaxUsers)
                {
                    ModelState.AddModelError(string.Empty, $"Limite de usuários atingido ({tenant.MaxUsers}). Entre em contato para ampliar seu plano.");
                    var departments = await _departmentService.FindAllAsync();
                    var viewModel = new AttorneyFormViewModel { Attorney = attorney, Departments = departments };
                    return View(viewModel);
                }
            }

            await _attorneyService.InsertAsync(attorney);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Id not provided"});
            }
            var obj = await _attorneyService.FindByIdAsync(id.Value);
            if (obj == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Id not found" });
            }
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _attorneyService.RemoveAsync(id);
                return RedirectToAction(nameof(Index));
            }catch (IntegrityException ex)
            {
                return RedirectToAction(nameof(Error), new { message = ex.Message });
            }          
        }

        public async Task<IActionResult> Details(int? id)
        {

            if (id == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Id not provided" }); ;
            }
            var obj = await _attorneyService.FindByIdAsync(id.Value);
            if (obj == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Id not found" });
            }
            return View(obj);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Id not provided" });
            }
            var obj = await _attorneyService.FindByIdAsync(id.Value);
            if (obj == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Id not found" });
            }
            Attorney usuario = _isessao.BuscarSessaoDoUsuario();
            ViewBag.CurrentUserPerfil = usuario.Perfil;
            List<Department> departments = await _departmentService.FindAllAsync();
            AttorneyFormViewModel viewModel = new AttorneyFormViewModel { Attorney = obj, Departments = departments };
            return View(viewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Attorney attorney)             
        {
            Attorney usuario = _isessao.BuscarSessaoDoUsuario();
            ViewBag.LoggedUserId = usuario.Id;
            ViewBag.CurrentUserPerfil = usuario.Perfil;

            // Valida��o: somente Admin pode criar outro usu�rio com perfil de Admin
            if (usuario.Perfil != ProfileEnum.Admin && attorney.Perfil == ProfileEnum.Admin)
            {
                ModelState.AddModelError(string.Empty, "Voc� n�o tem permiss�o para alterar um usu�rio com perfil de Administrador.");
                var departments = await _departmentService.FindAllAsync();
                var viewModel = new AttorneyFormViewModel { Attorney = attorney, Departments = departments };
                return View(viewModel);
            }

            if (id != attorney.Id) 
            { 
                return RedirectToAction(nameof(Error), new { message = "Id not mismatch" });
            }

            // Valida��o: verificar se o email j� existe (excluindo o pr�prio usu�rio)
            var emailExiste = await _attorneyService.EmailExistsAsync(attorney.Email, attorney.Id);
            if (emailExiste)
            {
                ModelState.AddModelError("Attorney.Email", "Este email j� est� em uso. Por favor, use outro email.");
                var departments = await _departmentService.FindAllAsync();
                var viewModel = new AttorneyFormViewModel { Attorney = attorney, Departments = departments };
                return View(viewModel);
            }

            try
            {                
                await _attorneyService.UpdateAsync(attorney);
                
                // Se editou o pr�prio usu�rio, atualiza a sess�o
                if (usuario.Id == attorney.Id)
                {
                    var atualizado = _attorneyService.ListarPorId(attorney.Id);
                    if (atualizado != null)
                    {
                        atualizado.Department = null; // evita loop circular na serializa��o
                        _isessao.CriarSessaoDoUsuario(atualizado);
                    }
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException ex)
            {
                return RedirectToAction(nameof(Error), new { message = ex.Message });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erro ao salvar: " + ex.Message);
                var departments = await _departmentService.FindAllAsync();
                var viewModel = new AttorneyFormViewModel { Attorney = attorney, Departments = departments };
                return View(viewModel);
            }
        }

        public IActionResult Error (string message)
        {
            var viewModel = new ErrorViewModel
            {
               Message = message,
               RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(viewModel);
        }

    }
}
