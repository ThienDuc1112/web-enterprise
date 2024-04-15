using Microsoft.EntityFrameworkCore;
using WebEnterprise.Infrastructure.Persistance;
using WebEnterprise.Models.Entities;
using WebEnterprise.Repositories.Abstraction;

namespace WebEnterprise.Repositories.Implement
{
    public class MegazineRepository : GenericRepository<Megazine>, IMegazineRepository
    {
        private readonly UniversityDbContext _dbContext;
        public MegazineRepository(UniversityDbContext businessDbContext) : base(businessDbContext)
        {
            _dbContext = businessDbContext;
        }

        public async Task<List<Megazine>> GetMegazinesWithRelevant(string query)
        {
            IQueryable<Megazine> meQuery = _dbContext.Megazines;

            if (!string.IsNullOrEmpty(query))
            {
                meQuery = meQuery.Where(c => c.Name.Contains(query));
            }

            var megazines = await meQuery.Include(f => f.Faculty).ToListAsync();
            return megazines;
        }
    }
}
