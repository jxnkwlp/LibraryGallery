using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;
using Passingwind.LibraryGallery.Domains;

namespace Passingwind.LibraryGallery.Services
{
    public class GithubService : IGithubService
    {
        private readonly ILogger<GithubService> _logger;
        private readonly UserManager _userManager;
        private readonly ILibraryService _libraryService;

        public GithubService(UserManager userManager, ILogger<GithubService> logger, ILibraryService libraryService)
        {
            _userManager = userManager;
            _logger = logger;
            _libraryService = libraryService;
        }

        public async Task<string> GetTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception($"user '{userId}' not found.");

            var claims = await _userManager.GetClaimsAsync(user);

            return claims.FirstOrDefault(x => x.Type == "urn:github:access_token")?.Value ?? null;
        }

        public async Task<IReadOnlyList<GithubRepository>> GetStarredListAsync(string userId)
        {
            var result = new List<GithubRepository>();

            var token = await GetTokenAsync(userId);

            if (string.IsNullOrEmpty(token))
                throw new Exception($"user '{userId}' token not found.");

            var credentials = new Credentials(token);

            var connection = new Connection(new ProductHeaderValue("Passingwind-LibraryGallery"))
            {
                Credentials = credentials
            };

            var apiConnection = new ApiConnection(connection);

            var github = new GitHubClient(connection);

            var user = await github.User.Current();

            _logger.LogInformation($"Loading user Name:'{user.Name}', Login:{user.Login} star repositories.");

            var a = new RepositoriesClient(apiConnection);
            StarredClient starredClient = new StarredClient(apiConnection);

            try
            {
                var sw = Stopwatch.StartNew();

                var request = new StarredRequest()
                {
                    SortDirection = SortDirection.Ascending,
                    SortProperty = StarredSort.Created,
                };

                //var list = await starredClient.GetAllForCurrent(new StarredRequest()
                //{
                //    SortDirection = SortDirection.Descending,
                //    SortProperty = StarredSort.Created,
                //}, new ApiOptions() { PageSize = 100 });


                var list = await apiConnection.GetAll<Repository>(new Uri($"https://api.github.com/users/{user.Login}/starred"), request.ToParametersDictionary(), accepts: "application/vnd.github.mercy-preview+json", options: new ApiOptions() { PageSize = 100, });

                sw.Stop();

                _logger.LogInformation($"Load starred list cost : {sw.Elapsed}");

                foreach (var item in list)
                {
                    var repo = new GithubRepository()
                    {
                        FullName = item.FullName,
                        Name = item.Name,
                        Description = item.Description,
                        Url = item.HtmlUrl,
                        IsFork = item.Fork,
                        ForksCount = item.ForksCount,
                        OpenIssuesCount = item.OpenIssuesCount,
                        StargazersCount = item.StargazersCount,
                        WatchersCount = item.WatchersCount,
                        Homepage = item.FullName,
                        Language = item.Homepage,
                        License = item.License?.Name,
                    };

                    repo.Topics = item.Topics?.ToArray();

                    result.Add(repo);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public async Task SyncAsync(string userId, Action<string> syncMessageAction)
        {
            try
            {
                syncMessageAction?.Invoke($"Try get starred repos... ");

                var repos = await GetStarredListAsync(userId);

                syncMessageAction?.Invoke($"Get starred repo: {repos.Count}. ");

                var existingList = await _libraryService.GetListAsync(userId);

                foreach (var item in repos)
                {
                    if (existingList.Any(x => x.Title == item.FullName) == false)
                    {
                        var lib = new Library()
                        {
                            Title = item.FullName,
                            Description = item.Description,
                            Link = item.Url,
                            Type = LibraryType.GithubStar,
                            UserId = userId,
                        };

                        if (item.Topics != null)
                        {
                            item.Topics.ToList().ForEach(t =>
                            {
                                lib.AddTags(t);
                            });
                        }

                        await _libraryService.AddAsync(lib);

                        _logger.LogInformation($"Added repo {item.FullName}.");
                        syncMessageAction?.Invoke($"Added repo {item.FullName}.");
                    }
                    else
                    {
                        _logger.LogInformation($"Repo {item.FullName} exists. ");
                        syncMessageAction?.Invoke($"Repo {item.FullName} exists. ");
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sync user started repos error.");
                syncMessageAction?.Invoke(ex.Message);
            }
        }

        public class Topic
        {
            public string[] Names { get; set; }
        }
    }

    public class GithubRepository
    {
        public string FullName { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Homepage { get; set; }
        public string Language { get; set; }
        public int ForksCount { get; set; }
        public int StargazersCount { get; set; }
        public int WatchersCount { get; set; }
        public int OpenIssuesCount { get; set; }
        public bool IsFork { get; set; }
        public string License { get; set; }
        public string[] Topics { get; set; }
    }
}
