using Serilog.Core;
using Serilog.Events;

namespace Sorigin.Utilities;

public class EndpointTimeLogFilter : ILogEventFilter
{
    public bool IsEnabled(LogEvent logEvent)
    {
        LogEventPropertyValue? sourceProp = logEvent.Properties.GetValueOrDefault("SourceContext");
        if (sourceProp is null || sourceProp.ToString() != "Microsoft.AspNetCore.Hosting.Diagnostics")
            return true;

        string renderedMessage = logEvent.RenderMessage();
        return renderedMessage.EndsWith("ms");
    }
}