using Serilog.Core;
using Serilog.Events;

namespace Sorigin.Utilities;

public class EndpointTimeLogEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        LogEventPropertyValue? sourceProp = logEvent.Properties.GetValueOrDefault("SourceContext");
        if (sourceProp is null || sourceProp.ToString().EndsWith("Diagnostics"))
            return;

        var elapsedProp = logEvent.Properties.GetValueOrDefault("ElapsedMilliseconds");
        var pathProp = logEvent.Properties.GetValueOrDefault("RequestPath");

        if (elapsedProp is null || pathProp is null)
            return;

        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("HostingRequestFinishedLog", string.Format("{0} took {1}ms to execute.", pathProp.ToString(), elapsedProp.ToString())));
    }
}
