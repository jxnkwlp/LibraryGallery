using System.Collections.Generic;
using System.Threading.Tasks;
using Passingwind.LibraryGallery.Domains;

namespace Passingwind.LibraryGallery.Services
{
    public interface ILibraryService
    {
        Task<int> GetCountAsync(string userId, string filter = null);
        Task<List<Library>> GetListAsync(string userId, string filter = null);
        Task<List<Library>> GetPagedListAsync(int skipCount, int maxResultCount, string userId, string filter = null);

        Task AddAsync(Library library);
    }
}
