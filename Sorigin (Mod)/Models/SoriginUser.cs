using System;

namespace Sorigin.Models
{
    public class SoriginUser
    {
        public Guid ID { get; set; }
        public Role Role { get; set; }
        public string Username { get; set; } = null!;
        public GamePlatform GamePlatform { get; set; }

        public DiscordUser? Discord { get; set; }
        public SteamUser? Steam { get; set; }

        public string GetProfilePicture(Size size = Size.Medium)
        {
            if (Discord != null)
            {
                if (size == Size.Small)
                    return "https://sorigin.org" + Discord.AvatarURL + "?size=128";
                if (size == Size.Medium)
                    return "https://sorigin.org" + Discord.AvatarURL + "?size=256";
                if (size == Size.Large)
                    return "https://sorigin.org" + Discord.Avatar + "?size=1024";
            }
            else if (Steam != null)
            {
                if (size == Size.Small)
                    return $"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/{GetFTC(Steam.Avatar)}/{Steam.Avatar}.jpg";
                if (size == Size.Medium)
                    return $"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/{GetFTC(Steam.Avatar)}/{Steam.Avatar}_medium.jpg";
                if (size == Size.Large)
                    return $"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/{GetFTC(Steam.Avatar)}/{Steam.Avatar}_full.jpg";
            }
            return "TODO: Use some sort of default profile picture? This really should never be seen since a platform is required to have an account. But kris is not done with the logo yet so...";
        }

        /// <summary>
        /// Gets the first two characters in a string
        /// </summary>
        private string GetFTC(string input)
        {
            return input.Substring(0, 2);
        }

        public enum Size
        {
            Small,
            Medium,
            Large
        }
    }
}