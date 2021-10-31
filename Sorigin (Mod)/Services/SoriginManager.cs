using IPA.Utilities.Async;
using Newtonsoft.Json;
using SiraUtil.Logging;
using SiraUtil.Web;
using Sorigin.Models;
using System;
using System.Threading.Tasks;
using Zenject;

namespace Sorigin.Services
{
    internal class SoriginManager : ISoriginManager, IInitializable, ITickable, IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly IHttpService _httpService;
        private readonly IPlatformUserModel _platformUserModel;
        private readonly SoriginGrantService _soriginGrantService;
        private readonly SoriginNetworkService _soriginNetworkService;
        private DateTime? _sessionStarted;
        private bool _didSteam;

        public event Action? SessionExpired;
        public event Action<SoriginUser>? LoggedIn;

        public string? Token { get; private set; }
        public SoriginUser? Player { get; private set; }

        internal SoriginManager(SiraLog siraLog, IHttpService httpService, IPlatformUserModel platformUserModel, SoriginGrantService soriginGrantService, SoriginNetworkService soriginNetworkService)
        {
            _siraLog = siraLog;
            _httpService = httpService;
            _platformUserModel = platformUserModel;
            _soriginGrantService = soriginGrantService;
            _soriginNetworkService = soriginNetworkService;
        }

        public void Initialize()
        {
            _soriginNetworkService.TokenReceived += SoriginNetworkService_TokenReceived;
            _soriginGrantService.GrantReceived += SoriginGrantService_GrantReceived;
            Task.Run(FetchBySteam);
        }

        private async void SoriginGrantService_GrantReceived(string grant)
        {
            try
            {
                // If we've been given a grant and there's already a user logged in, transfer the two accounts.
                if (Player != null)
                {
                    var response = await _httpService.PostAsync($"https://sorigin.org/api/transfer?grant={grant}&platform=discord");

                    if (!response.Successful)
                    {
                        _siraLog.Error("Account transfer failed!");
                        _siraLog.Error(await response.ReadAsStringAsync());
                        return;
                    }


                    SoriginUser user = JsonConvert.DeserializeObject<SoriginUser>(await response.ReadAsStringAsync());
                    Player = user;
                    LoggedIn?.Invoke(Player);
                }
            }
            catch (Exception e)
            {
                _siraLog.Error(e);
            }
        }

        private async Task FetchBySteam()
        {
            if (_platformUserModel is SteamPlatformUserModel)
            {
                PlatformUserAuthTokenData tokenData = await _platformUserModel.GetUserAuthToken();
                if (tokenData.validPlatformEnvironment == PlatformUserAuthTokenData.PlatformEnviroment.Production)
                {
                    _siraLog.Debug("Attempting to login via Steam.");
                    UserInfo userInfo = await _platformUserModel.GetUserInfo();
                    _didSteam = await _soriginNetworkService.LoginThroughSteam(userInfo.platformUserId, tokenData.token);
                }
            }
        }

        private async void SoriginNetworkService_TokenReceived(string token)
        {
            try
            {
                _siraLog.Debug("Token has been received. Fetching user profile.");
                _httpService.Token = token;
                var response = await _httpService.SendAsync(HTTPMethod.GET, "https://sorigin.org/api/auth/@me");
                if (!response.Successful)
                    return;

                if (Token != null)
                {
                    _siraLog.Notice("Killing session...");
                    MainThread(() => SessionExpired?.Invoke());
                }

                _siraLog.Debug("Deserialzing user");
                SoriginUser user = JsonConvert.DeserializeObject<SoriginUser>(await response.ReadAsStringAsync());
                _sessionStarted = DateTime.Now;
                Player = user;
                Token = token;

                _siraLog.Info($"Successfully logged in '{user.Username}'.");
                MainThread(() => LoggedIn?.Invoke(Player));
                
                if (!_didSteam)
                {
                    if (_platformUserModel is SteamPlatformUserModel)
                    {
                        _siraLog.Debug("Fetching Steam auth token.");
                        PlatformUserAuthTokenData tokenData = await _platformUserModel.GetUserAuthToken();
                        if (tokenData.validPlatformEnvironment == PlatformUserAuthTokenData.PlatformEnviroment.Production)
                        {
                            _siraLog.Debug("Trying to add Steam as a platform...");
                            await _soriginNetworkService.LoginThroughSteam(token, tokenData.token);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _siraLog.Error(e);
            }
        }

        public void Tick()
        {
            if (_sessionStarted.HasValue)
            {
                if (DateTime.Now > _sessionStarted.Value.AddHours(4) && Player != null)
                {
                    _siraLog.Debug("Ending the user session.");
                    SessionExpired?.Invoke();
                    _sessionStarted = null;
                    Player = null;
                    Token = null;
                }
            }
        }

        public void Dispose()
        {
            _soriginNetworkService.TokenReceived -= SoriginNetworkService_TokenReceived;
            _soriginGrantService.GrantReceived -= SoriginGrantService_GrantReceived;
        }

        private static void MainThread(Action action)
        {
            UnityMainThreadTaskScheduler.Factory.StartNew(action);
        }
    }
}