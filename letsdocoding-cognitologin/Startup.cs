using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace letsdocoding_cognitologin
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
            services.AddControllersWithViews();
            services.AddHttpContextAccessor();
            //Add authentication
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                //Add cookie
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                // Add openid connect
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.ClientId = Configuration.GetSection("Identity:Cognito:ClientId").Value;
                    options.ClientSecret = Configuration.GetSection("Identity:Cognito:ClientSecret").Value;
                    options.Authority = Configuration.GetSection("Identity:Cognito:Authority").Value;
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.ResponseType = "code";
                    options.SaveTokens = true;

                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        NameClaimType = Configuration.GetSection("Identity:UserNameClaim").Value
                    };

                    //
                    options.Events = new OpenIdConnectEvents()
                    {
                        OnRedirectToIdentityProviderForSignOut = context =>
                        {
                            var logoutUri = $"{Configuration.GetSection("Identity:Cognito:LogoutUri").Value}/logout?client_id={Configuration.GetSection("Identity:Cognito:ClientId").Value}";

                            logoutUri += $"&logout_uri={context.Request.Scheme}://{context.Request.Host}";
                            context.Response.Redirect(logoutUri);
                            context.HandleResponse();

                            return Task.CompletedTask;
                        }
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //Add Authentication
            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
