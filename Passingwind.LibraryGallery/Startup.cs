using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Passingwind.LibraryGallery.Data;
using Passingwind.LibraryGallery.Domains;
using Passingwind.LibraryGallery.Identity;
using Passingwind.LibraryGallery.Services;

namespace Passingwind.LibraryGallery
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("Default"), b =>
                {
                    b.MigrationsAssembly("Passingwind.LibraryGallery");
                });
            });

            services
                .AddIdentity<User, Role>(options =>
                {
                })
                .AddUserManager<UserManager>()
                .AddRoleManager<RoleManager>()
                .AddSignInManager<SignInManager>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication()
                .AddCookie()
                .AddGitHub("github", options =>
                 {
                     options.ClientId = Configuration["Authentication:GitHub:ClientId"];
                     options.ClientSecret = Configuration["Authentication:GitHub:ClientSecret"];
                     options.Events.OnCreatingTicket = (context) =>
                     {
                         context.Identity.AddClaim(new Claim("urn:github:access_token", context.AccessToken));

                         return Task.CompletedTask;
                     };
                 });

            services.AddTransient<ILibraryService, LibraryService>();
            services.AddTransient<IGithubService, GithubService>();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.ForwardLimit = 1;
                options.KnownProxies.Clear();
                options.KnownNetworks.Clear();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/debug")
                {
                    context.Response.ContentType = "text/plain";

                    // Request method, scheme, and path
                    await context.Response.WriteAsync(
                        $"Request Method: {context.Request.Method}{Environment.NewLine}");
                    await context.Response.WriteAsync(
                        $"Request Scheme: {context.Request.Scheme}{Environment.NewLine}");
                    await context.Response.WriteAsync(
                        $"Request Path: {context.Request.Path}{Environment.NewLine}");
                    // Connection: RemoteIp
                    await context.Response.WriteAsync(
                        $"Request RemoteIp: {context.Connection.RemoteIpAddress}{Environment.NewLine}");

                    // Headers
                    await context.Response.WriteAsync(Environment.NewLine);
                    await context.Response.WriteAsync($"Request Headers:{Environment.NewLine}");

                    foreach (var header in context.Request.Headers)
                    {
                        await context.Response.WriteAsync($"--- {header.Key}: " +
                            $"{header.Value}{Environment.NewLine}");
                    }

                }
                else
                    await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


#if DEBUG
            app.UseHttpsRedirection();
#endif
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
