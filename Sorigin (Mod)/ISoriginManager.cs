using Sorigin.Models;
using System;

namespace Sorigin
{
    public interface ISoriginManager
    {
        event Action<SoriginUser>? LoggedIn;
        event Action? SessionExpired;

        string? Token { get; }
        SoriginUser? Player { get; }
    }
}