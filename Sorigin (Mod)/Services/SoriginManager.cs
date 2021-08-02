using Newtonsoft.Json;
using SiraUtil.Tools;
using Sorigin.Models;
using System;
using System.Threading.Tasks;
using Zenject;

namespace Sorigin.Services
{
    internal class SoriginManager : ISoriginManager, IInitializable, ITickable, IDisposable
    {
        private readonly Http _http;
        private readonly SiraLog _siraLog;
        private readonly IPlatformUserModel _platformUserModel;
        private readonly SoriginNetworkService _soriginNetworkService;
        private DateTime? _sessionStarted;

        public event Action? SessionExpired;
        public event Action<SoriginUser>? LoggedIn;

        public string? Token { get; private set; }
        public SoriginUser? Player { get; private set; }

        internal SoriginManager(Http http, SiraLog siraLog, IPlatformUserModel platformUserModel, SoriginNetworkService soriginNetworkService)
        {
            _http = http;
            _siraLog = siraLog;
            _platformUserModel = platformUserModel;
            _soriginNetworkService = soriginNetworkService;
        }

        public void Initialize()
        {
            _soriginNetworkService.TokenReceived += SoriginNetworkService_TokenReceived;
            Task.Run(FetchBySteam);
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
                    await _soriginNetworkService.LoginThroughSteam(userInfo.platformUserId, tokenData.token);
                }
            }
        }

        private async void SoriginNetworkService_TokenReceived(string token)
        {
            try
            {
                _siraLog.Debug("Token has been received. Fetching user profile.");
                var response = await _http.GetAsync("https://sorigin.org/api/auth/@me", token);
                if (!response.Successful)
                    return;

                if (Token != null)
                {
                    _siraLog.Logger.Notice("Killing session...");
                    SessionExpired?.Invoke();
                }

                _siraLog.Debug("Deserialzing ser");
                SoriginUser user = JsonConvert.DeserializeObject<SoriginUser>(response.Content!);
                _sessionStarted = DateTime.Now;
                Player = user;
                Token = token;

                _siraLog.Info($"Successfully logged in '{user.Username}'.");

                _siraLog.Debug("Sending out log in event.");
                LoggedIn?.Invoke(Player);
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
                    Player = null;
                    Token = null;
                }
            }
        }

        public void Dispose()
        {
            _soriginNetworkService.TokenReceived -= SoriginNetworkService_TokenReceived;
        }
    }
}