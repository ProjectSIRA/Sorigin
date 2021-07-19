using Microsoft.EntityFrameworkCore;
using Sorigin.Models.Platforms;
using System;
using System.Text.Json.Serialization;

namespace Sorigin.Models
{
    [Index(nameof(Username))]
    public class User
    {
        public Guid ID { get; set; }
        public Role Role { get; set; }
        public string Username { get; set; } = null!;
        
        [JsonIgnore]
        public string Hash { get; set; } = null!;

        // <---- Platforms ---->

        public DiscordUser? Discord { get; set; }
    }
}