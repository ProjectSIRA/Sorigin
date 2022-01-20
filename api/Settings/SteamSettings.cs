namespace Sorigin.Settings;

public interface ISteamSettings
{
    string Key { get; }
    string AppID { get; }
}

internal class SteamSettings : ISteamSettings
{
    public string Key { get; init; } = null!;
    public string AppID { get; init; } = null!;
}
