using Newtonsoft.Json;
using SiraUtil.Logging;
using SiraUtil.Web;
using System;
using System.Threading.Tasks;
using Zenject;

namespace Sorigin.Services
{
    internal class SoriginNetworkService : IInitializable, IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly IHttpService _httpService;
        public event Action<string>? TokenReceived;
        private readonly SoriginGrantService _soriginGrantService;

        public SoriginNetworkService(SiraLog siraLog, IHttpService httpService, SoriginGrantService soriginGrantService)
        {
            _siraLog = siraLog;
            _httpService = httpService;
            _soriginGrantService = soriginGrantService;
        }

        public async Task<bool> LoginThroughSteam(string id, string token)
        {
            token = token.Replace("-", string.Empty);
            /*_siraLog.Debug("Checking if the user exists in the Steam database...");
            var response = await _httpService.GetAsync($"https://sorigin.org/api/user/by-steam/{id}");
            if (!response.Successful)
            {
                _siraLog.Debug("This user doesn't have their Steam linked to a Sorigin account.");
                return false;
            }*/

            _siraLog.Debug("Great, their Steam account is connected. Authorizing using Steam token...");
            var response = await _httpService.PostAsync("https://sorigin.org/api/auth/login?grant=" + token + "&platform=steam");
            if (!response.Successful)
            {
                _siraLog.Warn("The Steam token could not be authorized. Either the web server is down, the Steam session is invalid (unlikely), or the user has pirated the game.");
                return false;
            }

            _siraLog.Debug("Sorigin token received (Steam). Sending out event.");
            string tokenString = await response.ReadAsStringAsync();
            TokenBody tokens = JsonConvert.DeserializeObject<TokenBody>(tokenString);
            TokenReceived?.Invoke(tokens.Token);
            return true;
        }

        public void Initialize()
        {
            _soriginGrantService.GrantReceived += SoriginGrantService_GrantReceived;
        }

        private void SoriginGrantService_GrantReceived(string grant)
        {
            // try
            // {
            //     _siraLog.Debug("Grant received! Exchanging it for a token.");
            //     var response = await _httpService.PostAsync($"https://sorigin.org/api/auth/login?grant={grant}&platform=discord");
            //     if (!response.Successful)
            //     {
            //         _siraLog.Error($"Could not process grant!.");
            //         return;
            //     }
            // 
            //     _siraLog.Debug("Sorigin token received (Discord). Sending out event.");
            //     string tokenString = await response.ReadAsStringAsync();
            //     TokenBody tokens = JsonConvert.DeserializeObject<TokenBody>(tokenString);
            //     TokenReceived?.Invoke(tokens.Token);
            // }
            // catch (Exception e)
            // {
            //     _siraLog.Error(e);
            // }
        }

        public void Dispose()
        {
            _soriginGrantService.GrantReceived -= SoriginGrantService_GrantReceived;
        }

        internal class TokenBody
        {
            public string Token { get; set; } = null!;
            public string RefreshToken { get; set; } = null!;
        }
    }
}