using System;

namespace Sorigin
{
    public class AuthorizedSoriginUser
    {
        public string Token { get; }
        public SoriginUser User { get; }
        public DateTimeOffset Expiration { get; }

        internal AuthorizedSoriginUser(string token, SoriginUser user, DateTimeOffset expiration)
        {
            User = user;
            Token = token;
            Expiration = expiration;
        }
    }
}