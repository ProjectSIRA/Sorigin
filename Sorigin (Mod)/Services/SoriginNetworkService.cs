using Newtonsoft.Json;
using SiraUtil.Tools;
using System;
using System.Threading.Tasks;
using Zenject;

namespace Sorigin.Services
{
    internal class SoriginNetworkService : IInitializable, IDisposable
    {
        private readonly Http _http;
        private readonly SiraLog _siraLog;
        public event Action<string>? TokenReceived;
        private readonly SoriginGrantService _soriginGrantService;

        public SoriginNetworkService(Http http, SiraLog siraLog, SoriginGrantService soriginGrantService)
        {
            _http = http;
            _siraLog = siraLog;
            _soriginGrantService = soriginGrantService;
        }

        public async Task LoginThroughSteam(string id, string token)
        {
            _siraLog.Debug("Checking if the user exists in the Steam database...");
            var response = await _http.GetAsync($"https://sorigin.org/api/user/by-steam/{id}");
            if (!response.Successful)
            {
                _siraLog.Debug("This user doesn't have their Steam linked to a Sorigin account.");
                return;
            }

            _siraLog.Debug("Great, their Steam account is connected. Authorizing using Steam token...");
            response = await _http.PostAsync("https://sorigin.org/api/auth/token", JsonConvert.SerializeObject(new { platform = 1, token }));
            if (!response.Successful)
            {
                _siraLog.Warning("The Steam token could not be authorized. Either the web server is down, the Steam session is invalid (unlikely), or the user has pirated the game.");
                return;
            }

            _siraLog.Debug("Sorigin token received (Steam). Sending out event.");
            TokenReceived?.Invoke(JsonConvert.DeserializeObject<TokenBody>(response.Content!).Token);
        }

        public void Initialize()
        {
            _soriginGrantService.GrantReceived += SoriginGrantService_GrantReceived;
        }

        private async void SoriginGrantService_GrantReceived(string grant)
        {
            try
            {
                _siraLog.Debug("Grant received! Exchanging it for a token.");
                var response = await _http.PostAsync("https://sorigin.org/api/auth/token", JsonConvert.SerializeObject(new { platform = 0, token = grant }));
                if (!response.Successful)
                {
                    _siraLog.Error($"Could not process grant! {response.Content}.");
                    return;
                }

                _siraLog.Debug("Sorigin token received (Discord). Sending out event.");
                TokenReceived?.Invoke(JsonConvert.DeserializeObject<TokenBody>(response.Content!).Token);
            }
            catch (Exception e)
            {
                _siraLog.Error(e);
            }
        }

        public void Dispose()
        {
            _soriginGrantService.GrantReceived -= SoriginGrantService_GrantReceived;
        }

        internal class TokenBody
        {
            public string Token { get; set; } = null!;
        }
    }
}