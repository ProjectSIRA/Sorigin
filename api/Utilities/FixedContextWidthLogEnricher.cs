using Serilog.Core;
using Serilog.Events;

namespace Sorigin.Utilities;

public class FixedContextWidthLogEnricher : ILogEventEnricher
{
    private readonly int _width;

    public FixedContextWidthLogEnricher(int width)
    {
        _width = width;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var property = logEvent.Properties.GetValueOrDefault("SourceContext");
        string typeName = "";
        if (property is not null)
        {
            typeName = property.ToString().Replace("\"", string.Empty);
            if (typeName.Contains('.'))
            {
                typeName = typeName[(typeName.LastIndexOf('.') + 1)..];
            }
        }

        if (typeName.Length > _width)
            typeName = typeName[^_width..];
        else if (typeName.Length < _width)
            typeName = new string(' ', _width - typeName.Length) + typeName;
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("SourceContext", typeName));
    }
}
