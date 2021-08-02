namespace Sorigin.Models
{
    public class DiscordUser
    {
        public string ID { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Discriminator { get; set; } = null!;
        public string AvatarURL { get; set; } = null!;
        public string Avatar { get; set; } = null!;

        public string FormattedName()
        {
            return $"{Username}#{Discriminator}";
        }
    }
}