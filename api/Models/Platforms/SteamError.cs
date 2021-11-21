using System.Text.Json.Serialization;

namespace Sorigin.Models.Platforms;

public class SteamError
{
    [JsonPropertyName("errorcode")]
    public int Code { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;
}