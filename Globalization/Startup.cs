using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Globalization.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Routing;

namespace Globalization
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddJsonLocalization(options => options.ResourcesPath = "Resources");
            services.AddMvc().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
               .AddDataAnnotationsLocalization();
            //services.AddScoped<LanguageActionFilter>();
            
            services.Configure<RequestLocalizationOptions>(
            options =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("vn-VN"),
                };      

                options.DefaultRequestCulture = new RequestCulture(culture: "vn-VN", uiCulture: "vn-VN");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders = new[]
                {
                    new RouteDataRequestCultureProvider
                    {
                        IndexOfCulture=1,
                        IndexofUICulture=1
                    }
                };
            });

            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap.Add("culture", typeof(LanguageRouteConstraint));
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                name: "LocalizedDefault",
                template: "{culture}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{*catchall}",
                    defaults: new
                    {
                        controller = "Home",
                        action = "RedirectToDefaultLanguage",
                        culture = "vn-VN"
                    });
            });
        }

        public class LanguageRouteConstraint : IRouteConstraint
        {
            public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
            {
                if (!values.ContainsKey("culture"))
                    return false;

                var culture = values["culture"].ToString();
                return culture == "en-US" || culture == "vn-VN";
            }
        }

        public class RouteDataRequestCultureProvider : RequestCultureProvider
        {
            public int IndexOfCulture;
            public int IndexofUICulture;
            public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
            {
                if (httpContext == null)
                    throw new ArgumentNullException(nameof(httpContext));
                string culture = null;
                string uiCulture = null;
                var twoLetterCultureName = httpContext.Request.Path.Value.Split('/')[IndexOfCulture]?.ToString();
                var twoLetterUICultureName = httpContext.Request.Path.Value.Split('/')[IndexofUICulture]?.ToString();
                if (twoLetterCultureName == "vn")
                    culture = "vn-VN";
                else if (twoLetterCultureName == "en")
                    culture = uiCulture = "en-US";
                if (twoLetterUICultureName == "vn")
                    culture = "vn-VN";
                else if (twoLetterUICultureName == "en")
                    culture = uiCulture = "en-US";
                if (culture == null && uiCulture == null)
                    return NullProviderCultureResult;

                if (culture != null && uiCulture == null)
                    uiCulture = culture;
                if (culture == null && uiCulture != null)
                    culture = uiCulture;
                var providerResultCulture = new ProviderCultureResult(culture, uiCulture);
                return Task.FromResult(providerResultCulture);
            }
        }
    }
}
