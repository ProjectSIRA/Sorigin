using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sorigin.Authorization;
using Sorigin.Services;
using Sorigin.Settings;
using System;

namespace Sorigin
{
    public class Startup
    {
        private const string CORSOrigins = "_allowSOrigins";
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JWTSettings>(Configuration.GetSection(nameof(JWTSettings)));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<JWTSettings>>().Value);
            var deploymentSettings = Configuration.GetSection(nameof(DeploymentSettings)).Get<DeploymentSettings>();

            services.AddHttpClient();
            
            services.AddScoped<IAuthService, SoriginAuthService>();

            services.AddSingleton<IPasswordHasher, BCryptNETPasswordHasher>();

            services.AddDbContext<SoriginContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("Default"));
                options.UseSnakeCaseNamingConvention();
            });

            services.AddCors(options =>
            {
                options.AddPolicy(name: CORSOrigins, opt =>
                {
                    opt.WithOrigins(deploymentSettings.CORS)
                    .AllowAnyHeader().AllowAnyMethod();
                });
            });

            services.AddControllers();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearerConfiguration(Configuration["JWTSettings:Issuer"], Configuration["JWTSettings:Audience"], Configuration["JWTSettings:Key"]);
            services.AddAuthorization(options =>
            {
                Array.ForEach(Scopes.AllScopes, scope =>
                    options.AddPolicy(scope,
                        policy => policy.Requirements.Add(new ScopeRequirement(Configuration["JWTSettings:Issuer"], scope))));
            });
            services.AddSingleton<IAuthorizationHandler, RequireScopeHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SoriginContext soriginContext, ILogger<Startup> logger)
        {
            logger.LogDebug("Ensuring that the database is created...");
            soriginContext.Database.EnsureCreated();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(CORSOrigins);

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallback(async context => await context.Response.WriteAsync("Not Found"));
                endpoints.Map("/api", async context => await context.Response.WriteAsync("Sorigin API OK"));
            });
        }
    }
}