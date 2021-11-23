using System;

namespace Sorigin
{
    public interface ISoriginService
    {
        event Action? OnLogout;
        event Action<AuthorizedSoriginUser>? OnLogin;
        AuthorizedSoriginUser? User { get; }
    }
}