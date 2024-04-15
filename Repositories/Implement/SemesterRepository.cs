using Microsoft.EntityFrameworkCore;
using WebEnterprise.Infrastructure.Persistance;
using WebEnterprise.Models.Entities;
using WebEnterprise.Repositories.Abstraction;

namespace WebEnterprise.Repositories.Implement
{
    public class SemesterRepository : GenericRepository<Semester>, ISemesterRepository
    {
        private UniversityDbContext _dbContext;
        public SemesterRepository(UniversityDbContext businessDbContext) : base(businessDbContext)
        {
            _dbContext = businessDbContext;
        }

        public async Task<List<string>> GetThreeNewestSemester()
        {
            return await _dbContext.Semesters
                .OrderBy(s => s.Id)
                .Take(3)
                .Select(s => s.Name)
                .ToListAsync();
        }

        public async Task<bool> IsExisted(string name)
        {
            return await _dbContext.Faculties.AllAsync(x => x.Name == name);
        }
    }   
}
