using Newtonsoft.Json;
using SiraUtil.Logging;
using SiraUtil.Web;
using SiraUtil.Zenject;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace Sorigin.Services
{
    internal class SoriginService : IAsyncInitializable, ITickable, ISoriginService
    {
        private bool _loggedIn = false;
        private readonly SiraLog _siraLog;
        private readonly IHttpService _httpService;
        private readonly IPlatformUserModel _platformUserModel;

        public event Action? OnLogout;

        private event Action<AuthorizedSoriginUser>? OnLoginInternal;

        public event Action<AuthorizedSoriginUser>? OnLogin
        {
            add
            {
                OnLoginInternal += value;
                if (_loggedIn && User != null)
                    value?.Invoke(User);
            }
            remove => OnLoginInternal -= value;
        }

        public AuthorizedSoriginUser? User { get; private set; }

        public SoriginService(SiraLog siraLog, IHttpService httpService, IPlatformUserModel platformUserModel)
        {
            _siraLog = siraLog;
            _httpService = httpService;
            _platformUserModel = platformUserModel;
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            if (_platformUserModel is SteamPlatformUserModel)
            {
                PlatformUserAuthTokenData tokenData = await _platformUserModel.GetUserAuthToken();
                if (token.IsCancellationRequested)
                    return;

                if (tokenData == null)
                {
                    _siraLog.Warn("Unable to get Steam auth token. Is this a pirated game install? Is the user's internet working?");
                    return;
                }

                IHttpResponse response = await _httpService.PostAsync("https://sorigin.org/api/login/steam", new SoriginTokenBody { Token = tokenData.token.Replace("-", string.Empty) }, token);
                if (!response.Successful)
                {
                    _siraLog.Warn("Unable to login to Sorigin through Steam.");
                    try
                    {
                        string bodyString = await response.ReadAsStringAsync();
                        SoriginErrorBody body = JsonConvert.DeserializeObject<SoriginErrorBody>(bodyString);
                        _siraLog.Warn("Error: " + body.Error + " | " + body.ErrorMessage);
                    }
                    catch
                    {
                        _siraLog.Warn("Error: " + await response.Error());
                    }
                    return;
                }

                string soriginToken = JsonConvert.DeserializeObject<SoriginTokenBody>(await response.ReadAsStringAsync()).Token;
                _siraLog.Debug("Acquired Sorigin Token. Fetching user account data.");

                _httpService.Token = soriginToken;
                response = await _httpService.GetAsync("https://sorigin.org/api/users/@me", cancellationToken: token);
                _httpService.Token = null;

                if (!response.Successful)
                {
                    _siraLog.Critical("Unable to get the Sorigin User! This should't happen under normal circumstances.");
                    return;
                }

                SoriginUser soriginUser = JsonConvert.DeserializeObject<SoriginUser>(await response.ReadAsStringAsync());
                User = new AuthorizedSoriginUser(soriginToken, soriginUser, DateTime.UtcNow.AddHours(4));
                _loggedIn = true;

                OnLoginInternal?.Invoke(User);
                _siraLog.Notice($"Logged in as {soriginUser.Username}.");
            }
        }

        public void Tick()
        {
            if (_loggedIn)
            {
                if (User != null)
                {
                    if (DateTime.UtcNow > User.Expiration)
                    {
                        User = null;
                        _loggedIn = false;
                        _siraLog.Notice("User has been logged out due to session expiration.");
                        OnLogout?.Invoke();

                    }
                }
                else
                {
                    _loggedIn = false;
                    _siraLog.Warn("Sorigin user has been forcibly logged out.");
                    OnLogout?.Invoke();
                }
            }
        }

        private class SoriginErrorBody
        {
            [JsonProperty("error")]
            public string Error { get; set; } = null!;

            [JsonProperty("errorMessage")]
            public string ErrorMessage { get; set; } = null!;
        }

        private class SoriginTokenBody
        {
            [JsonProperty("token")]
            public string Token { get; set; } = null!;
        }
    }
}