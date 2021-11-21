using Sorigin.Settings;

namespace Sorigin.Models;

public interface ISoriginSettings
{
    ISteamSettings Steam { get; }
    IJWTSettings JWT { get; }
}

internal class SoriginSettings : ISoriginSettings
{
    [ConfigurationKeyName("Steam")]
    public SteamSettings SteamSettings { get; init; } = null!;

    [ConfigurationKeyName("JWT")]
    public JWTSettings JWTSettings { get; init; } = null!;

    public ISteamSettings Steam => SteamSettings;
    public IJWTSettings JWT => JWTSettings;
}