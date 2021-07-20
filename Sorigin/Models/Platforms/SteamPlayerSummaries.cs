using System;
using System.Text.Json.Serialization;

namespace Sorigin.Models.Platforms
{
    public class SteamPlayerSummaries
    {
        [JsonPropertyName("players")]
        public SteamPlayer[] Players { get; set; } = Array.Empty<SteamPlayer>();
    }
}