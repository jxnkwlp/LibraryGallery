using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Passingwind.LibraryGallery.Domains;
using Passingwind.LibraryGallery.Identity;

namespace Passingwind.LibraryGallery.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager _signInManager;
        private readonly UserManager _userManager;

        public AuthController(SignInManager signInManager, ILogger<AuthController> logger, UserManager userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> LoginAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            string provider = "github";

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, Url.Action("GithubLoginCallback", "Auth", null, protocol: Request.Scheme));

            return Challenge(properties, provider);
        }

        public async Task<IActionResult> LogoutAsync()
        {
            if (_signInManager.IsSignedIn(User))
            {
                await _signInManager.SignOutAsync();
            }

            return LocalRedirect("/");
        }

        public async Task<IActionResult> GithubLoginCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                return Content($"Error from external provider: {remoteError}");
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Content("Error loading external login information.");
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);

                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                var claims = await _userManager.GetClaimsAsync(user);
                await _userManager.RemoveClaimsAsync(user, claims);

                // save
                var storeCliams = info.Principal.Claims.Where(x => x.Type != ClaimTypes.NameIdentifier && x.Type != ClaimTypes.Name && x.Type != ClaimTypes.Email).ToArray();
                await _userManager.AddClaimsAsync(user, storeCliams);

                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return Redirect("./Lockout");
            }
            else
            {
                var githubId = info.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var name = info.Principal.FindFirst(ClaimTypes.Name)?.Value;
                var email = info.Principal.FindFirst(ClaimTypes.Email)?.Value;

                // return Json(info.Principal.Claims.Select(x => new { x.Type, x.Value }));

                var user = new Domains.User()
                {
                    Email = email,
                    UserName = email,
                };
                var identityResult = await _userManager.CreateAsync(user);

                if (identityResult.Succeeded)
                {
                    await _userManager.AddLoginAsync(user, info);

                    // save
                    var storeCliams = info.Principal.Claims.Where(x => x.Type != ClaimTypes.NameIdentifier && x.Type != ClaimTypes.Name && x.Type != ClaimTypes.Email).ToArray();
                    await _userManager.AddClaimsAsync(user, storeCliams);

                    // login
                    await _signInManager.SignInAsync(user, true);
                }
            }

            return LocalRedirect("~/");
        }

    }
}
