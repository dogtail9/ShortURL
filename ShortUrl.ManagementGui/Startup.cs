// https://stigrune.dev/posts/adding-new-OpenAPI-service-references-to-Core-projects
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IdentityModel.Client;

namespace ShortUrl.ManagementGui
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

            // Add the authentication service
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = Configuration.GetConnectionString("SecurityTokenService"); 
                    options.RequireHttpsMetadata = false;

                    options.ClientId = "managementguiclient";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";

                    options.SaveTokens = true;
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("offline_access");
                    options.Scope.Add("managementapi");
                });

            services.AddAccessTokenManagement(options =>
            {
                // client config is inferred from OpenID Connect settings
                // if you want to specify scopes explicitly, do it here, otherwise the scope parameter will not be sent
                options.Client.Scope = "managementapi";

            })
                .ConfigureBackchannelHttpClient();

            //services.AddAccessTokenManagement(options =>
            //{
            //    options.Client.Clients.Add("identityserver", new ClientCredentialsTokenRequest
            //    {
            //        Address = $"{Configuration.GetConnectionString("SecurityTokenService")}connect/token",
            //        ClientId = "managementapiclient",
            //        ClientSecret = "secret",
            //        Scope = "managementapi"
            //    });
            //});

            services.AddHttpClient<IManagementApiClient, ManagementApiClient>(client =>
            {
                client.BaseAddress = new Uri(Configuration.GetConnectionString("ManagementService"));
            })
                .AddClientAccessTokenHandler();
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
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
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
