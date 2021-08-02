﻿using System;

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
    }
}