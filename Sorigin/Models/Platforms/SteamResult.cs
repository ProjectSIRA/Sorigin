using System.Text.Json.Serialization;

namespace Sorigin.Models.Platforms
{
    public class SteamResult
    {
        [JsonPropertyName("error")]
        public SteamError? Error { get; set; }

        [JsonPropertyName("params")]
        public SteamAuthParams? Params { get; set; }
    }
}