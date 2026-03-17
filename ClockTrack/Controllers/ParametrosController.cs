using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClockTrack.Data;
using ClockTrack.Models;
using ClockTrack.Helper;
using static ClockTrack.Helper.Sessao;

namespace ClockTrack.Controllers
{
    public class ParametrosController : Controller
    {
        private readonly ClockTrackContext _context;
        private readonly ISessao _isessao;

        public ParametrosController(ClockTrackContext context, ISessao isessao)
        {
            _context = context;
            _isessao = isessao;
        }

        // GET: Parametros
        public async Task<IActionResult> Index()
        {
            try
            {
                Attorney usuario = _isessao.BuscarSessaoDoUsuario();
                ViewBag.LoggedUserId = usuario.Id;
                ViewBag.CurrentUserPerfil = usuario.Perfil;
                
                var parametros = await _context.Parametros.ToListAsync();
                bool canCreate = !parametros.Any(); // Permitir criar somente se n�o houver nenhum par�metro
                ViewBag.CanCreate = canCreate;
                return View(parametros);
            }
            catch (SessionExpiredException)
            {
                // Redirecione para a p�gina de login se a sess�o expirou
                TempData["MensagemAviso"] = "A sess�o expirou. Por favor, fa�a login novamente.";
                return RedirectToAction("Index", "Login");
            }
        }

        // GET: Parametros/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Parametros == null)
            {
                return NotFound();
            }

            var parametros = await _context.Parametros
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parametros == null)
            {
                return NotFound();
            }

            return View(parametros);
        }

        // GET: Parametros/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Width,Height")] Parametros parametros, IFormFile logo)
        {
                ModelState.Remove("Logo");
                ModelState.Remove("Tenant");

                if (logo != null)
                {
                    if (logo.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("Logo", "A imagem deve ter no máximo 5MB.");
                        return View(parametros);
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await logo.CopyToAsync(memoryStream);
                        parametros.LogoData = memoryStream.ToArray();
                        parametros.LogoMimeType = logo.ContentType;
                    }

                    _context.Add(parametros);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("Logo", "O campo Logo é obrigatório.");
                }
           // }
            return View(parametros);
        }


        // GET: Parametros/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Parametros == null)
            {
                return NotFound();
            }

            var parametros = await _context.Parametros.FindAsync(id);
            if (parametros == null)
            {
                return NotFound();
            }
            return View(parametros);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Width,Height")] Parametros parametros, IFormFile logo)
        {
            if (id != parametros.Id)
            {
                return NotFound();
            }

           // if (ModelState.IsValid)
           //{
                try
                {
                    ModelState.Remove("Logo");
                    ModelState.Remove("Tenant");
                    // Se um novo logo for carregado, substitua o existente
                    if (logo != null)
                    {
                        if (logo.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("Logo", "A imagem deve ter no máximo 5MB.");
                            return View(parametros);
                        }

                        using (var memoryStream = new MemoryStream())
                        {
                            await logo.CopyToAsync(memoryStream);
                            parametros.LogoData = memoryStream.ToArray();
                            parametros.LogoMimeType = logo.ContentType;
                        }
                    }
                    else
                    {
                        // Mantenha os dados do logo atual se um novo logo n�o for carregado
                        var existingParametros = await _context.Parametros.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                        if (existingParametros != null)
                        {
                            parametros.LogoData = existingParametros.LogoData;
                            parametros.LogoMimeType = existingParametros.LogoMimeType;
                        }
                    }

                    _context.Update(parametros);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParametrosExists(parametros.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            //}
            //return View(parametros);
        }


        // GET: Parametros/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Parametros == null)
            {
                return NotFound();
            }

            var parametros = await _context.Parametros
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parametros == null)
            {
                return NotFound();
            }

            return View(parametros);
        }

        // POST: Parametros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Parametros == null)
            {
                return Problem("Entity set 'ClockTrackContext.Parametros'  is null.");
            }
            var parametros = await _context.Parametros.FindAsync(id);
            if (parametros != null)
            {
                _context.Parametros.Remove(parametros);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParametrosExists(int id)
        {
          return (_context.Parametros?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
