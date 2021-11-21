namespace Sorigin.Settings;

public interface IJWTSettings
{
    string Key { get; }
    string Issuer { get; }
    string Audience { get; }

    /// <summary>
    /// In Hours!!!
    /// </summary>
    float TokenLifetime { get; }
}

internal class JWTSettings : IJWTSettings
{
    public string Key { get; init; } = null!;

    public string Issuer { get; init; } = null!;

    public string Audience { get; init; } = null!;

    public float TokenLifetime { get; init; } = 4f;
}