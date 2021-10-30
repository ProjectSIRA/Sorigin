﻿using System.Text.Json.Serialization;

namespace Sorigin.Models.Platforms
{
    public class DiscordUser
    {
        public string Id { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Discriminator { get; set; } = null!;
        public string Avatar { get; set; } = null!;

        [JsonPropertyName("avatarURL")]
        public string ProfileURL
        {
            get
            {
                if (Avatar.StartsWith("/"))
                    return Avatar;
                return "https://cdn.discordapp.com/avatars/" + Id + "/" + Avatar + (Avatar.Substring(0, 2) == "a_" ? ".gif" : ".png");
            }
        }

        public string FormattedName()
        {
            return $"{Username}#{Discriminator}";
        }
    }
}