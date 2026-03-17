using Microsoft.EntityFrameworkCore;
using ClockTrack.Data;
using ClockTrack.Models;

namespace ClockTrack.Services
{
    public class ValorClienteService
    {
        private readonly ClockTrackContext _context;

        public ValorClienteService(ClockTrackContext context)
        {
            _context = context;
        }
        public async Task<ValorCliente> GetValorForClienteAndUserAsync(int clientId, int userId)
        {
            return await _context.ValorCliente
                .Where(p => p.ClientId == clientId && p.AttorneyId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<ValorCliente> GetValorForClienteAndUserIdAsync(int clientId, int userId)
        {
            return await _context.ValorCliente
                .Include(p => p.Client)
                .Include(p => p.Attorney)
                .Where(p => p.ClientId == clientId && p.AttorneyId == userId)
                .FirstOrDefaultAsync();
        }

    }
}
