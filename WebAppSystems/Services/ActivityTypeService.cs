using Microsoft.EntityFrameworkCore;
using WebAppSystems.Data;
using WebAppSystems.Models;

namespace WebAppSystems.Services
{
    public class ActivityTypeService
    {
        private readonly WebAppSystemsContext _context;

        public ActivityTypeService(WebAppSystemsContext context)
        {
            _context = context;
        }

        public async Task<List<ActivityType>> FindAllAsync()
        {
            return await _context.ActivityTypes
                .Where(a => a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();
        }

        public async Task<ActivityType> FindByIdAsync(int id)
        {
            return await _context.ActivityTypes.FindAsync(id);
        }

        public async Task InsertAsync(ActivityType activityType)
        {
            _context.ActivityTypes.Add(activityType);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ActivityType activityType)
        {
            _context.ActivityTypes.Update(activityType);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ActivityTypes.AnyAsync(e => e.Id == id);
        }

        public async Task<bool> IsInUseAsync(int id)
        {
            return await _context.ProcessRecord.AnyAsync(p => p.ActivityTypeId == id);
        }

        // Soft delete - apenas desativa
        public async Task DeactivateAsync(int id)
        {
            var activityType = await FindByIdAsync(id);
            if (activityType != null)
            {
                activityType.IsActive = false;
                await UpdateAsync(activityType);
            }
        }
    }
}
