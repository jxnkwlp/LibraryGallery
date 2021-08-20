using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Passingwind.LibraryGallery.Data;
using Passingwind.LibraryGallery.Domains;
using Passingwind.LibraryGallery.Extensions;

namespace Passingwind.LibraryGallery.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly AppDbContext _dbContext;

        public LibraryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Library library)
        {
            _dbContext.Add(library);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetCountAsync(string userId, string filter = null)
        {
            return await _dbContext.Libraries
                   .WhereIf(!string.IsNullOrEmpty(userId), x => x.UserId == userId)
                   .WhereIf(!string.IsNullOrEmpty(filter), x => EF.Functions.Like(x.Title, $"%{filter}%") || EF.Functions.Like(x.Description, $"%{filter}%"))
                   .CountAsync();
        }

        public async Task<List<Library>> GetListAsync(string userId, string filter = null)
        {
            return await _dbContext.Libraries
                    .Include(x => x.Tags)
                   .WhereIf(!string.IsNullOrEmpty(userId), x => x.UserId == userId)
                   .WhereIf(!string.IsNullOrEmpty(filter), x => EF.Functions.Like(x.Title, $"%{filter}%") || EF.Functions.Like(x.Description, $"%{filter}%"))
                   .ToListAsync();
        }

        public async Task<List<Library>> GetPagedListAsync(int skipCount, int maxResultCount, string userId, string filter = null)
        {
            return await _dbContext.Libraries
                .Include(x => x.Tags)
                      .WhereIf(!string.IsNullOrEmpty(userId), x => x.UserId == userId)
                      .WhereIf(!string.IsNullOrEmpty(filter), x => EF.Functions.Like(x.Title, $"%{filter}%") || EF.Functions.Like(x.Description, $"%{filter}%"))
                      .OrderByDescending(x => x.Id)
                      .Skip(skipCount)
                      .Take(maxResultCount)
                      .ToListAsync();
        }
    }
}
