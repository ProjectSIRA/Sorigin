using DryIoc;
using Serilog;
using Sorigin;
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

container.RegisterContextedLogger();
container.RegisterDelegate(() => Assembly.GetExecutingAssembly().GetName().Version, Reuse.Singleton);

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
    endpoints.Map("/", async (context) =>
    {
        Version version = context.RequestServices.GetRequiredService<Version>();
        string versionText = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        await context.Response.WriteAsJsonAsync(new { status = "HEALTHY", version = versionText });
    });
});

app.Run();