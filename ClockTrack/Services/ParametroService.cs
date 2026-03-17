using Microsoft.EntityFrameworkCore;
using ClockTrack.Data;
using ClockTrack.Models;
using ClockTrack.Models.Dto;

namespace ClockTrack.Services
{
    public interface IParametroService
    {
        Task<(byte[] ImageData, string MimeType, int Width, int Height)> GetLogoAsync();
    }

    public class ParametroService : IParametroService
    {
        private readonly ClockTrackContext _context;

        public ParametroService(ClockTrackContext context)
        {
            _context = context;
        }

        public async Task<(byte[] ImageData, string MimeType, int Width, int Height)> GetLogoAsync()
        {
            var parametros = await _context.Parametros.FirstOrDefaultAsync();
            if (parametros == null || parametros.LogoData == null)
            {
                throw new Exception("Configuração de logo não encontrada.");
            }

            return (parametros.LogoData, parametros.LogoMimeType, parametros.Width, parametros.Height);
        }
    }
}
