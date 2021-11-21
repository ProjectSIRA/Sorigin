namespace Sorigin.Settings;

public interface IAdminSettings
{
    string ProxyIP { get; }
    string OverrideKey { get; }
}

internal class AdminSettings : IAdminSettings
{
    public string ProxyIP { get; init; } = null!;
    public string OverrideKey { get; init; } = null!;
}