using DryIoc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NodaTime;
using Serilog;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Sorigin;
using Sorigin.Models;
using Sorigin.Services;
using Sorigin.Settings;
using Sorigin.Utilities;
using System.Reflection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
PhysicalFileProvider fileProvider = new PhysicalFileProvider(builder.Configuration.GetSection(nameof(Sorigin)).GetSection("Path")["FileRoot"]);
IJWTSettings jwtSettings = builder.Configuration.GetRequiredSection(nameof(Sorigin)).GetRequiredSection("JWT").Get<JWTSettings>();
IContainer container = builder.Host.UseSoriginDryIoC();
builder.Configuration.UseSoriginLogger();

// -------------------------------------
// 1: Service and Container Registration
// -------------------------------------

builder.Host.UseSerilog();
builder.Services.AddHttpClient();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().ConfigureSoriginJSON();
builder.Services.AddSwaggerGen(c => c.ConfigureSoriginJSON());
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearerConfiguration(jwtSettings.Issuer, jwtSettings.Audience, jwtSettings.Key);
builder.Services.AddDbContext<SoriginContext>(o => { o.UseNpgsql(builder.Configuration.GetConnectionString("Default"), o => o.UseNodaTime()); o.UseSnakeCaseNamingConvention(); });
builder.Services.AddImageSharp().ClearProcessors().AddProcessor<TieredResizeWebProcessor>().ClearProviders().AddProvider<SoriginPhysicalFileProvider>();

container.RegisterContextedLogger();
container.RegisterDelegate<IClock>(() => SystemClock.Instance);
container.RegisterConfig<ISoriginSettings, SoriginSettings>(builder.Configuration, nameof(Sorigin));
container.RegisterDelegate<ISoriginSettings, IMaxMindSettings>(s => s.MaxMind);
container.RegisterDelegate<ISoriginSettings, IAdminSettings>(s => s.Admin);
container.RegisterDelegate<ISoriginSettings, ISteamSettings>(s => s.Steam);
container.RegisterDelegate<ISoriginSettings, IPathSettings>(s => s.Path);
container.RegisterDelegate<ISoriginSettings, IJWTSettings>(s => s.JWT);
container.RegisterDelegate(() => Assembly.GetExecutingAssembly().GetName().Version, Reuse.Singleton);
container.Register<ITokenService, SoriginTokenService>(Reuse.Singleton);
container.Register<ILocationService, LocationService>(Reuse.Singleton);
container.Register<ISteamService, SteamService>(Reuse.Singleton);
container.Register<IStreamHasher, StreamHasher>(Reuse.Singleton);
container.Register<IFileService, FileService>(Reuse.Singleton);
container.Register<IMediaService, MediaService>(Reuse.Scoped);
container.Register<IUserService, UserService>(Reuse.Scoped);

builder.Environment.WebRootPath = fileProvider.Root;
builder.Environment.WebRootFileProvider = fileProvider;
// --------------------------------
// 2: Configuring the HTTP Pipeline
// --------------------------------

WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseImageSharp();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = fileProvider,
    RequestPath = "/cdn"
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.Map("/api/umbra", async context => await context.Response.WriteAsync("umbra smelly"));
    endpoints.MapFallback(async context => await context.Response.WriteAsJsonAsync(new { error = "Not Found", errorMessage = "Endpoint doesn't exist." }));
    endpoints.Map("/api", async (context) =>
    {
        Version version = context.RequestServices.GetRequiredService<Version>();
        string versionText = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        await context.Response.WriteAsJsonAsync(new { status = "HEALTHY", version = versionText });
    });
    endpoints.Map("/api/garsh", async context => await context.Response.WriteAsync((await context.RequestServices.GetRequiredService<ISteamService>().GetProfileFromID(76561198187936410))!.Username));
});

await app.Services.MigrateSorigin();
await app.Services.PopulateDefaultImages();
await app.RunAsync();