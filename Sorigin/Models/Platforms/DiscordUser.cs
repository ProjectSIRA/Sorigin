using System.Text.Json.Serialization;

namespace Sorigin.Models.Platforms
{
    public record DiscordUser(string Id, string Username, string Discriminator, string Avatar)
    {
        [JsonPropertyName("avatarURL")]
        public string ProfileURL
        {
            get => "https://cdn.discordapp.com/avatars/" + Id + "/" + Avatar + (Avatar.Substring(0, 2) == "a_" ? ".gif" : ".png");
        }

        public string FormattedName()
        {
            return $"{Username}#{Discriminator}";
        }
    }
}