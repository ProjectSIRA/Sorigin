using DryIoc;
using Serilog;
using Sorigin;


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
app.UseAuthorization();
app.MapControllers();

app.Run();