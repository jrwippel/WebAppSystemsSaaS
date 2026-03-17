using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClockTrack.Data;
using ClockTrack.Filters;
using ClockTrack.Helper;
using ClockTrack.Models;
using ClockTrack.Services.Exceptions;
using ClockTrack.Services;
using System.Diagnostics;
using static ClockTrack.Helper.Sessao;

namespace ClockTrack.Controllers
{
    [PaginaParaUsuarioLogado]
    [PaginaRestritaSomenteAdmin]
    public class DepartmentsController : Controller
    {
        private readonly ClockTrackContext _context;
        private readonly DepartmentService _departmentService;
        private readonly ISessao _isessao;

        public DepartmentsController(ClockTrackContext context, DepartmentService departmentService, ISessao isessao)
        {
            _context = context;
            _departmentService = departmentService;
            _isessao = isessao;
        }
        /*
        public DepartmentsController(ClockTrackContext context)
        {
            _context = context;
        }
        */

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            try
            {
                Attorney usuario = _isessao.BuscarSessaoDoUsuario();
                ViewBag.LoggedUserId = usuario.Id;
                ViewBag.CurrentUserPerfil = usuario.Perfil;

                return _context.Department != null ?
                            View(await _context.Department.ToListAsync()) :
                            Problem("Entity set 'ClockTrackContext.Department'  is null.");
            }
            catch (SessionExpiredException)
            {
                // Redirecione para a página de login se a sessão expirou
                TempData["MensagemAviso"] = "A sessão expirou. Por favor, faça login novamente.";
                return RedirectToAction("Index", "Login");
            }
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Department == null)
            {
                return NotFound();
            }

            var department = await _context.Department
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Department == null)
            {
                return NotFound();
            }

            var department = await _context.Department.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Departments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Department department)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Buscar o departamento existente para preservar o TenantId
                    var existingDepartment = await _context.Department.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
                    if (existingDepartment != null)
                    {
                        department.TenantId = existingDepartment.TenantId; // Preserva TenantId original
                    }

                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _departmentService.RemoveAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (IntegrityException ex)
            {
                return RedirectToAction(nameof(Error), new { message = ex.Message });
            }

        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Department == null)
            {
                return Problem("Entity set 'ClockTrackContext.Department'  is null.");
            }
            var department = await _context.Department.FindAsync(id);
            if (department != null)
            {
                _context.Department.Remove(department);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
          return (_context.Department?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        public IActionResult Error(string message)
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
