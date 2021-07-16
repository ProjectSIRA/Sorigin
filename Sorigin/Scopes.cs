using Sorigin.Models;

namespace Sorigin
{
    public static class Scopes
    {
        public static readonly string None = Role.None.ToString();
        public static readonly string Owner = Role.Owner.ToString();
        public static readonly string Admin = Role.Admin.ToString();
        public static readonly string Verified = Role.Verified.ToString();
        
        public static readonly string[] AllScopes =
        {
            None,
            Owner,
            Admin,
            Verified
        };
    }
}