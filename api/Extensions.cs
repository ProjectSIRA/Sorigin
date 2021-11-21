using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;
using Sorigin.Utilities;
using MLogger = Microsoft.Extensions.Logging.ILogger;

namespace Sorigin
{
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

            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().Enrich.With(new FixedContextWidthLogEnricher(w)).WriteTo.Async(a => a.Console(theme: AnsiConsoleTheme.Literate, outputTemplate: _outputTemplate)).ReadFrom.Configuration(configuration).CreateLogger();
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
    }
}