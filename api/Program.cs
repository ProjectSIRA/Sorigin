using DryIoc;
using Serilog;
using Sorigin;
using Sorigin.Models;
using Sorigin.Services;
using Sorigin.Settings;
using System.Reflection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
IContainer container = builder.Host.UseSoriginDryIoC();
builder.Configuration.UseSoriginLogger();

// -------------------------------------
// 1: Service and Container Registration
// -------------------------------------

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHttpClient();

container.RegisterContextedLogger();
container.RegisterConfig<ISoriginSettings, SoriginSettings>(builder.Configuration, nameof(Sorigin));
container.RegisterDelegate<ISoriginSettings, ISteamSettings>(s => s.Steam, Reuse.Singleton);

container.RegisterDelegate(() => Assembly.GetExecutingAssembly().GetName().Version, Reuse.Singleton);

container.Register<ISteamService, SteamService>(Reuse.Singleton);

// --------------------------------
// 2: Configuring the HTTP Pipeline
// --------------------------------

WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseEndpoints(endpoints =>
{
    endpoints.MapFallback(async context => await context.Response.WriteAsJsonAsync(new { error = "Not Found", errorMessage = "Endpoint doesn't exist." }));
    endpoints.Map("/api", async (context) =>
    {
        Version version = context.RequestServices.GetRequiredService<Version>();
        string versionText = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        await context.Response.WriteAsJsonAsync(new { status = "HEALTHY", version = versionText });
    });

    endpoints.Map("/api/umbra", async context => await context.Response.WriteAsync("umbra smelly"));
});

app.Run();