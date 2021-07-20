using System.Text.Json.Serialization;

namespace Sorigin.Models.Platforms
{
    public class SteamAuthParams
    {
        [JsonPropertyName("steamid")]
        public string SteamID { get; set; } = null!;
    }
}