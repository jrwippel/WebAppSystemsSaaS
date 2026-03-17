using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using ClockTrack.Data;
using ClockTrack.Models;
using ClockTrack.Models.Dto;
using ClockTrack.Services.Exceptions;

namespace ClockTrack.Services
{
    public class AttorneyService
    {
        private readonly ClockTrackContext _context;

        public AttorneyService(ClockTrackContext context)
        {
            _context = context;
        }

        public List<Attorney> FindAll()
        {
            return _context.Attorney.OrderBy(x => x.Name).ToList();
        }

        public async Task<List<Attorney>> FindAllAsync()
        {
            var attorneys = await _context.Attorney.ToListAsync();
            foreach (var attorney in attorneys)
            {
                var completeAttorney = await FindByIdAsync(attorney.Id);
                attorney.Department = completeAttorney.Department;
            }
            return attorneys;
        }

        public async Task<List<AttorneyDTO>> GetAllAttorneysAsync()
        {
            var attorneys = await _context.Attorney.ToListAsync();
            return attorneys.Select(a => new AttorneyDTO
            {
                Id = a.Id,
                Name = a.Name,
                DepartmentId = a.DepartmentId,
            }).ToList();
        }

        public Attorney ListarPorId(int id)
        {
            // IgnoreQueryFilters para funcionar antes do TenantId estar na sessão (ex: login)
            return _context.Attorney.IgnoreQueryFilters().FirstOrDefault(x => x.Id == id);
        }

        public async Task InsertAsync(Attorney obj)
        {
            obj.RegisterDate = DateTime.Now;
            obj.SetSenhaHash();
            _context.Add(obj);
            await _context.SaveChangesAsync();
        }

        public async Task<Attorney> FindByIdAsync(int id)
        {
            return await _context.Attorney.Include(obj => obj.Department).FirstOrDefaultAsync(obj => obj.Id == id);
        }

        public async Task RemoveAsync(int id)
        {
            try
            {
                var obj = await _context.Attorney.FindAsync(id);
                _context.Remove(obj);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new IntegrityException("Não é possível excluir esse usuário, pois ele possui atividades!");
            }
        }

        public async Task UpdateAsync(Attorney obj)
        {
            var hasAny = await _context.Attorney.AnyAsync(x => x.Id == obj.Id);
            if (!hasAny)
                throw new NotFoundException("Id not found");
            try
            {
                obj.UpdateDate = DateTime.Now;
                var existingAttorney = await _context.Attorney.AsNoTracking().FirstOrDefaultAsync(x => x.Id == obj.Id);
                obj.Password = existingAttorney.Password;
                obj.RegisterDate = existingAttorney.RegisterDate;
                obj.TenantId = existingAttorney.TenantId;
                obj.Department = null; // evita conflito de tracking na navigation property
                _context.Update(obj);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new DbConcurrencyException(ex.Message);
            }
        }

        public Attorney AtualizarSenha(Attorney usuario)
        {
            Attorney usuarioDB = ListarPorId(usuario.Id);
            if (usuarioDB == null) throw new Exception("Houve um erro na atualização do Usuario!");
            usuarioDB.Name = usuario.Name;
            usuarioDB.Email = usuario.Email;
            usuarioDB.Login = usuario.Login;
            usuarioDB.UpdateDate = DateTime.Now;
            usuarioDB.Perfil = usuario.Perfil;
            usuarioDB.Password = usuario.Password;
            usuarioDB.MustChangePassword = usuario.MustChangePassword;
            _context.Attorney.Update(usuarioDB);
            _context.SaveChanges();
            return usuarioDB;
        }

        public Attorney FindByLoginAsync(string login)
        {
            return _context.Attorney.IgnoreQueryFilters().FirstOrDefault(x => x.Login == login);
        }

        public Attorney FindByEmailAsync(string email)
        {
            return _context.Attorney.IgnoreQueryFilters().FirstOrDefault(x => x.Email.ToUpper() == email.ToUpper());
        }

        public Attorney BuscarPorEmailLogin(string email, string login)
        {
            return _context.Attorney.IgnoreQueryFilters().FirstOrDefault(x => x.Email.ToUpper() == email.ToUpper() && x.Login.ToUpper() == login.ToUpper());
        }

        public Attorney AlterarSenha(AlterarSenhaModel alterarSenhaModel)
        {
            Attorney usuarioDB = ListarPorId(alterarSenhaModel.Id);
            if (usuarioDB == null) throw new Exception("Houve um erro na atualização da senha, usuário não encontrado");
            if (!usuarioDB.ValidaSenha(alterarSenhaModel.SenhaAtual)) throw new Exception("Senha atual não confere");
            if (usuarioDB.ValidaSenha(alterarSenhaModel.SenhaNova)) throw new Exception("Senha nova deve ser diferente da atual");
            usuarioDB.SetNovaSenha(alterarSenhaModel.SenhaNova);
            _context.Attorney.Update(usuarioDB);
            _context.SaveChanges();
            return usuarioDB;
        }

        public bool IsValidUser(string username, string password)
        {
            try
            {
                Attorney usuario = FindByLoginAsync(username);
                if (usuario != null && usuario.ValidaSenha(password))
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void UpdateAttorney(Attorney attorney)
        {
            _context.Attorney.Update(attorney);
            _context.SaveChanges();
        }

        public async Task<bool> LoginExistsAsync(string login, int? excludeId = null)
        {
            var query = _context.Attorney.IgnoreQueryFilters().Where(a => a.Login.ToLower() == login.ToLower());
            if (excludeId.HasValue)
                query = query.Where(a => a.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var query = _context.Attorney.IgnoreQueryFilters().Where(a => a.Email.ToLower() == email.ToLower());
            if (excludeId.HasValue)
                query = query.Where(a => a.Id != excludeId.Value);
            return await query.AnyAsync();
        }
    }
}
