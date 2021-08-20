using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Passingwind.LibraryGallery.Services
{
    public interface IGithubService
    {
        Task<string> GetTokenAsync(string userId);
        Task<IReadOnlyList<GithubRepository>> GetStarredListAsync(string userId);
        Task SyncAsync(string userId,Action<string> syncMessageAction);
    }
}
