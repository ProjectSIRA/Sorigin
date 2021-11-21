using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;
using Sorigin.Utilities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using MLogger = Microsoft.Extensions.Logging.ILogger;

namespace Sorigin;

internal static class Extensions
{
    private static SerilogLoggerFactory? _serilogLoggerFactory;
    private const string _outputTemplate = "[{Timestamp:HH:mm:ss} | {Level:u3} | {SourceContext}] {Message:lj}{NewLine}{Exception}";

    public static MLogger ForContext(Type type)
    {
        return _serilogLoggerFactory!.CreateLogger(type);
    }

    public static IContainer UseSoriginDryIoC(this IHostBuilder host)
    {
        // We create the container, give it to the service provider factory, and return it so we can register our types.

        Container container = new();
        host.UseServiceProviderFactory(new DryIocServiceProviderFactory(container));
        return container;
    }

    public static void UseSoriginLogger(this IConfiguration configuration)
    {
        // We look for the "SourceContextWidth" in the Serilog configuration, and if not, we give it a default.
        string? width = configuration.GetSection("Serilog")["SourceContextWidth"];
        if (string.IsNullOrEmpty(width) || !int.TryParse(width, out int w) || w <= 0)
            w = 20;

        Log.Logger = new LoggerConfiguration().Filter.With(new EndpointTimeLogFilter()).Enrich.FromLogContext().Enrich.With(new EndpointTimeLogEnricher(), new FixedContextWidthLogEnricher(w)).WriteTo.Async(a => a.Console(theme: AnsiConsoleTheme.Literate, outputTemplate: _outputTemplate)).ReadFrom.Configuration(configuration).CreateLogger();
        _serilogLoggerFactory = new(Log.Logger);
    }

    public static void RegisterConfig<T, TImpl>(this IContainer container, IConfiguration configuration, string sectionName) where TImpl : class, T
    {
        IConfigurationSection section = configuration.GetSection(sectionName);
        container.RegisterInstance(new ConfigurationChangeTokenSource<TImpl>(Options.DefaultName, section));
        container.RegisterInstance<IConfigureOptions<TImpl>>(new NamedConfigureFromConfigurationOptions<TImpl>(Options.DefaultName, section, delegate { }));
        container.RegisterDelegate<IOptions<TImpl>, T>(s => s.Value, Reuse.Singleton);
    }

    public static void RegisterContextedLogger(this IContainer container)
    {
        container.Register(Made.Of(() => ForContext(Arg.Index<Type>(0)), r => r.Parent.ImplementationType), setup: Setup.With(condition: r => r.Parent.ImplementationType != null));
    }

    public static ulong? GetID(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(c => c.Type == ClaimTypes.NameIdentifier) ?? principal.FindFirst(c => c.Type == "sub");
        if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
        {
            bool didParse = ulong.TryParse(userIdClaim.Value, out ulong result);
            return didParse ? result : null;
        }
        return null;
    }

    // https://sandrino.dev/blog/aspnet-core-5-jwt-authorization
    public static AuthenticationBuilder AddJwtBearerConfiguration(this AuthenticationBuilder builder, string issuer, string audience, string key)
    {
        return builder.AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = issuer,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                ClockSkew = new TimeSpan(0, 0, 30),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
            };
            options.Events = new JwtBearerEvents()
            {
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    // Ensure we always have an error and error description.
                    if (string.IsNullOrEmpty(context.Error))
                        context.Error = "invalid_token";
                    if (string.IsNullOrEmpty(context.ErrorDescription))
                        context.ErrorDescription = "This request requires a valid JWT access token to be provided.";

                    // Add some extra context for expired tokens.
                    if (context.AuthenticateFailure != null && context.AuthenticateFailure.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        var authenticationException = (context.AuthenticateFailure as SecurityTokenExpiredException)!;
                        context.Response.Headers.Add("x-token-expired", authenticationException.Expires.ToString("o"));
                        context.ErrorDescription = $"The token expired on {authenticationException.Expires:o}";
                    }

                    return context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        error = context.Error,
                        errorMessage = context.ErrorDescription
                    }));
                }
            };
        });
    }

}
