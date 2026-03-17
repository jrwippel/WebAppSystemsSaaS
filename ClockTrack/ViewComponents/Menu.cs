using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ClockTrack.Data;
using ClockTrack.Models;

namespace ClockTrack.ViewComponents
{
    public class Menu : ViewComponent
    {
        private readonly ClockTrackContext _context;

        public Menu(ClockTrackContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            string sessaoUsuario = HttpContext.Session.GetString("sessaoUsuarioLogado");
            if (string.IsNullOrEmpty(sessaoUsuario)) 
                return View(new Attorney());
            
            Attorney attorney = JsonConvert.DeserializeObject<Attorney>(sessaoUsuario);
            
            // Calcular horas trabalhadas hoje (apenas registros finalizados)
            var today = DateTime.Today;
            var registrosHoje = await _context.ProcessRecord
                .Where(p => p.AttorneyId == attorney.Id 
                    && p.Date.Date == today 
                    && p.HoraFinal != TimeSpan.Zero 
                    && p.HoraFinal > p.HoraInicial) // Apenas registros finalizados
                .ToListAsync();
            
            var horasHoje = registrosHoje.Sum(p => (p.HoraFinal - p.HoraInicial).TotalHours);
            
            ViewBag.HorasHoje = horasHoje;
            
            // Buscar logo da empresa
            try
            {
                var parametros = await _context.Parametros.FirstOrDefaultAsync();
                ViewBag.CompanyLogo = parametros;
            }
            catch
            {
                ViewBag.CompanyLogo = null;
            }
            
            // Buscar nome do Tenant
            try
            {
                var tenant = await _context.Tenants.FindAsync(attorney.TenantId);
                ViewBag.TenantName = tenant?.Name ?? "Empresa";
            }
            catch
            {
                ViewBag.TenantName = "Empresa";
            }
            
            return View(attorney);
        }
    }
}
