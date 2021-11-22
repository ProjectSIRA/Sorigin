namespace Sorigin.Settings;

public interface IPathSettings
{
    string FileRoot { get; }
    string OculusImage { get; }
    string FallbackImage { get; }
}

internal class PathSettings : IPathSettings
{
    public string FileRoot { get; init; } = null!;
    public string OculusImage { get; init; } = null!;
    public string FallbackImage { get; init; } = null!;
}