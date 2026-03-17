using Microsoft.AspNetCore.Mvc;
using ClockTrack.Filters;
using ClockTrack.Helper;
using ClockTrack.Models;
using ClockTrack.Services;

namespace ClockTrack.Controllers
{
    [PaginaParaUsuarioLogado]
    [PaginaRestritaSomenteAdmin]
    public class ActivityTypesController : Controller
    {
        private readonly ActivityTypeService _service;

        public ActivityTypesController(ActivityTypeService service)
        {
            _service = service;
        }

        // GET: ActivityTypes
        public async Task<IActionResult> Index()
        {
            var activityTypes = await _service.FindAllAsync();
            return View(activityTypes);
        }

        // GET: ActivityTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ActivityTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActivityType activityType)
        {
            // Remove campos não enviados pelo form
            ModelState.Remove("TenantId");
            ModelState.Remove("Tenant");
            ModelState.Remove("ProcessRecords");

            if (ModelState.IsValid)
            {
                try
                {
                    await _service.InsertAsync(activityType);
                    TempData["MensagemSucesso"] = "Tipo de atividade criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao salvar: {ex.Message}");
                }
            }
            else
            {
                var erros = ModelState.Where(x => x.Value.Errors.Any())
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}");
                ModelState.AddModelError("", "Campos inválidos: " + string.Join(" | ", erros));
            }
            return View(activityType);
        }

        // GET: ActivityTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityType = await _service.FindByIdAsync(id.Value);
            if (activityType == null)
            {
                return NotFound();
            }
            return View(activityType);
        }

        // POST: ActivityTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ActivityType activityType)
        {
            if (id != activityType.Id)
            {
                return NotFound();
            }

            ModelState.Remove("TenantId");
            ModelState.Remove("Tenant");
            ModelState.Remove("ProcessRecords");

            if (ModelState.IsValid)
            {
                try
                {
                    await _service.UpdateAsync(activityType);
                    TempData["MensagemSucesso"] = "Tipo de atividade atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao salvar: {ex.Message}");
                }
            }
            else
            {
                var erros = ModelState.Where(x => x.Value.Errors.Any())
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}");
                ModelState.AddModelError("", "Campos inválidos: " + string.Join(" | ", erros));
            }
            return View(activityType);
        }

        // GET: ActivityTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityType = await _service.FindByIdAsync(id.Value);
            if (activityType == null)
            {
                return NotFound();
            }

            var isInUse = await _service.IsInUseAsync(id.Value);
            ViewBag.IsInUse = isInUse;

            return View(activityType);
        }

        // POST: ActivityTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var isInUse = await _service.IsInUseAsync(id);
                if (isInUse)
                {
                    // Soft delete - apenas desativa
                    await _service.DeactivateAsync(id);
                    TempData["MensagemSucesso"] = "Tipo de atividade desativado com sucesso!";
                }
                else
                {
                    TempData["MensagemErro"] = "Não é possível excluir este tipo de atividade pois está em uso.";
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = $"Erro ao excluir tipo de atividade: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
