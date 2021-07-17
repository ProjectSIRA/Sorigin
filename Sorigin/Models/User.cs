using System;
using System.Text.Json.Serialization;

namespace Sorigin.Models
{
    public class User
    {
        public Guid ID { get; set; }
        public string Username { get; set; } = null!;
        
        [JsonIgnore]
        public string Hash { get; set; } = null!;
    }
}