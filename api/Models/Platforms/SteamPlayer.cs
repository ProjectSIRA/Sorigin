using System.Text.Json.Serialization;

namespace Sorigin.Models.Platforms;

public class SteamPlayer
{
    [JsonPropertyName("steamid")]
    public string SteamID { get; set; } = null!;

    [JsonPropertyName("personaname")]
    public string PersonaName { get; set; } = null!;

    [JsonPropertyName("avatarhash")]
    public string AvatarHash { get; set; } = null!;
}