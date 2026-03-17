using Microsoft.EntityFrameworkCore;
using ClockTrack.Data;
using ClockTrack.Models;

namespace ClockTrack.Services
{
    public class MensalistaService
    {
        private readonly ClockTrackContext _context;

        public MensalistaService(ClockTrackContext context)
        {
            _context = context;
        }

        public async Task<List<Mensalista>> FindAllAsync()
        {
            // return await _context.Attorney.ToListAsync();
            var mensalistas = await _context.Mensalista.ToListAsync();
            return mensalistas;

        }

        public async Task<Mensalista> FindByIdAsync(int id)
        {
            return await _context.Mensalista.FirstOrDefaultAsync(mensalista => mensalista.Id == id);
        }

    }
}
