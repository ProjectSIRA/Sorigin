using Sorigin.Settings;

namespace Sorigin.Models;

public interface ISoriginSettings
{
    ISteamSettings Steam { get; }
}

internal class SoriginSettings : ISoriginSettings
{
    [ConfigurationKeyName("Steam")]
    public SteamSettings SteamSettings { get; init; } = null!;

    public ISteamSettings Steam => SteamSettings;
}