using Sorigin.Models;

namespace Sorigin
{
    public interface ISoriginManager
    {
        string? Token { get; }
        SoriginUser? Player { get; }
    }
}