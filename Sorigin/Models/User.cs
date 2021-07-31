using Microsoft.EntityFrameworkCore;
using Sorigin.Models.Platforms;
using System;

namespace Sorigin.Models
{
    [Index(nameof(Username))]
    public class User
    {
        public Guid ID { get; set; }
        public Role Role { get; set; }
        public string? Bio { get; set; }
        public string Username { get; set; } = null!;
        public GamePlatform GamePlatform { get; set; }

        // <---- Platforms ---->
        public DiscordUser? Discord { get; set; }
        public SteamUser? Steam { get; set; }
    }
}