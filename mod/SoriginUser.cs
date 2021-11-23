using System;

namespace Sorigin
{
    public class SoriginUser
    {
        public ulong ID { get; internal set; }

        public string Username { get; internal set; } = null!;
        public string ProfilePicture { get; internal set; } = null!;

        public DateTimeOffset Registration { get; internal set; }
        public DateTimeOffset LastLogin { get; internal set; }

        /// <summary>
        /// ISO Country Code. This is null if they don't have a country assigned.
        /// </summary>
        public string? Country { get; internal set; }
    }
}