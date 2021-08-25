using System;
using System.Collections.Generic;

namespace Sorigin.Services
{
    public interface IUserStateCache
    {
        Guid Add(string state);
        string? Pull(Guid id);
    }

    public class UserStateCache : IUserStateCache
    {
        private readonly Dictionary<Guid, string> _stateCache = new Dictionary<Guid, string>();

        public Guid Add(string state)
        {
            Guid id = Guid.NewGuid();
            _stateCache.Add(id, state);
            return id;
        }

        public string? Pull(Guid id)
        {
            if (_stateCache.TryGetValue(id, out string? state))
            {
                _stateCache.Remove(id);
                return state;
            }
            return null;
        }
    }
}