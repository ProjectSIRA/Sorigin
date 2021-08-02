using System;

namespace Sorigin.Models
{
    [Flags]
    public enum Role
    {
        None = 0,
        Owner = 1,
        Admin = 2,
        Verified = 4
    }
}
