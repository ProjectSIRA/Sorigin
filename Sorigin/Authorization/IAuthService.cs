using Sorigin.Models;
using System;
using System.Threading.Tasks;

namespace Sorigin.Authorization
{
    public interface IAuthService
    {
        Task<User?> GetUser(Guid? id);
        string Sign(Guid id, float lengthInHours = 1344f, Role role = Role.None);
    }
}