using Microsoft.EntityFrameworkCore;
using ClockTrack.Data;
using ClockTrack.Models;
using ClockTrack.Models.Dto;

namespace ClockTrack.Services
{
    public class ClientService
    {
        private readonly ClockTrackContext _context;

        public ClientService(ClockTrackContext context)
        {
            _context = context;
        }

        public async Task<Client> FindByIdAsync(int id)
        {
            return await _context.Client.FirstOrDefaultAsync(client => client.Id == id);
        }


        public async Task<List<Client>> FindAllAsync()
        {         
            var clients = await _context.Client.ToListAsync(); 
            return clients;
        }

        public async Task<List<ClientDTO>> GetAllClientsAsync()
        {
            var clients = await _context.Client.ToListAsync();
            return clients.Select(a => new ClientDTO
            {
                Id = a.Id,
                Name = a.Name,
                Solicitante = a.Solicitante,
            }).ToList();
        }

        public async Task<string> GetSolicitanteByClientIdAsync(int clientId)
        {
            var client = await _context.Client
                .Where(c => c.Id == clientId)
                .FirstOrDefaultAsync();

            if (client != null)
            {
                // Retorne o solicitante (ajuste conforme seu modelo de dados)
                return client.Solicitante;
            }

            return string.Empty;
        }


    }
}
