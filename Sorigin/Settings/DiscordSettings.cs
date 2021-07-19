using System;

namespace Sorigin.Settings
{
    public class DiscordSettings
    {
        public string ID { get; init; } = null!;
        public string URL { get; init; } = null!;
        public string Token { get; init; } = null!;
        public string Secret { get; init; } = null!;
        public string RedirectURL { get; init; } = null!;
        public string[] Roots { get; set; } = Array.Empty<string>();
    }
}