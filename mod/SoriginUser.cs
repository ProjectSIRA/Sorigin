using Newtonsoft.Json;
using System;

namespace Sorigin
{
    public class SoriginUser
    {
        [JsonProperty("id")]
        public ulong ID { get; internal set; }

        [JsonProperty("username")]
        public string Username { get; internal set; } = null!;

        [JsonProperty("profilePicture")]
        public string ProfilePicture { get; internal set; } = null!;

        [JsonProperty("registration")]
        public DateTimeOffset Registration { get; internal set; }
        [JsonProperty("lastLogin")]
        public DateTimeOffset LastLogin { get; internal set; }

        [JsonProperty("country")]
        /// <summary>
        /// ISO Country Code. This is null if they don't have a country assigned.
        /// </summary>
        public string? Country { get; internal set; }
    }
}