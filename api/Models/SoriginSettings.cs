using Sorigin.Settings;

namespace Sorigin.Models;

public interface ISoriginSettings
{
    IMaxMindSettings MaxMind { get; }
    IAdminSettings Admin { get; }
    ISteamSettings Steam { get; }
    IJWTSettings JWT { get; }
}

internal class SoriginSettings : ISoriginSettings
{
    [ConfigurationKeyName("MaxMind")]
    public MaxMindSettings MaxMindSettings { get; init; } = null!;

    [ConfigurationKeyName("Admin")]
    public AdminSettings AdminSettings { get; init; } = null!;

    [ConfigurationKeyName("Steam")]
    public SteamSettings SteamSettings { get; init; } = null!;

    [ConfigurationKeyName("JWT")]
    public JWTSettings JWTSettings { get; init; } = null!;

    public IMaxMindSettings MaxMind => MaxMindSettings;
    public IAdminSettings Admin => AdminSettings;
    public ISteamSettings Steam => SteamSettings;
    public IJWTSettings JWT => JWTSettings;
}