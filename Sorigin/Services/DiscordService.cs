using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sorigin.Models;
using Sorigin.Models.Platforms;
using Sorigin.Settings;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sorigin.Services
{
    public class DiscordService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _client;
        private readonly IMediaService _mediaService;
        private readonly SoriginContext _soriginContext;
        private readonly DiscordSettings _discordSettings;

        public DiscordService(ILogger<DiscordService> logger, HttpClient client, IMediaService mediaService, SoriginContext soriginContext, DiscordSettings discordSettings)
        {
            _logger = logger;
            _client = client;
            _mediaService = mediaService;
            _soriginContext = soriginContext;
            _discordSettings = discordSettings;
        }

        public async Task<string?> GetAccessToken(string code)
        {
            _logger.LogDebug("Fetching Access Token");
            Dictionary<string, string> parameters = new()
            {
                { "client_id", _discordSettings.ID },
                { "client_secret", _discordSettings.Secret },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _discordSettings.RedirectURL }
            };
            FormUrlEncodedContent content = new(parameters!);
            HttpResponseMessage response = await _client.PostAsync(_discordSettings.URL + "/oauth2/token", content);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Received Access Token");
                Stream responseStream = await response.Content.ReadAsStreamAsync();
                AccessTokenResponse? accessTokenResponse = await JsonSerializer.DeserializeAsync<AccessTokenResponse>(responseStream);
                return accessTokenResponse?.AccessToken;
            }
            _logger.LogWarning("Could not get access token. {ReasonPhrase}", response.ReasonPhrase);
            return null;
        }

        public async Task<DiscordUser?> GetProfile(string accessToken)
        {
            _logger.LogDebug("Getting active user profile.");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await _client.GetAsync(_discordSettings.URL + "/users/@me");
            if (response.IsSuccessStatusCode)
            {
                DiscordUser discordUser = await PopulateDiscordWithCachedProfilePicture(response);
                _logger.LogDebug("User Profile {Username}#{Discriminator} Found", discordUser?.Username, discordUser?.Discriminator);
                return discordUser;
            }
            _logger.LogWarning("Could not get user profile. {ReasonPhrase}", response.ReasonPhrase);
            return null;
        }

        public async Task<DiscordUser?> GetProfileFromID(string id)
        {
            _logger.LogDebug("Getting user profile ({ID})", id);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", _discordSettings.Token);
            HttpResponseMessage response = await _client.GetAsync(_discordSettings.URL + "/users/" + id);
            if (response.IsSuccessStatusCode)
            {
                DiscordUser discordUser = await PopulateDiscordWithCachedProfilePicture(response);
                _logger.LogDebug("User Profile {Username}#{Discriminator} Found", discordUser?.Username, discordUser?.Discriminator);
                return discordUser;
            }
            _logger.LogWarning("Could not get user profile. {ReasonPhrase}", response.ReasonPhrase);
            return null;
        }

        private async Task<DiscordUser> PopulateDiscordWithCachedProfilePicture(HttpResponseMessage response)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            DiscordUser? discordUser = JsonSerializer.Deserialize<DiscordUser>(responseString, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Media? media = await _soriginContext.Media.FirstOrDefaultAsync(m => m.Contract == discordUser!.Avatar);
            if (media is null)
            {
                string fileName = discordUser!.Avatar.StartsWith("a_") ? discordUser!.Avatar + ".gif" : discordUser!.Avatar + ".png";
                using Stream imageStream = await _client.GetStreamAsync(discordUser.ProfileURL + "?size=2048");
                using MemoryStream copyTo = new();
                await imageStream.CopyToAsync(copyTo);
                media = await _mediaService.Upload(fileName, copyTo, discordUser.Avatar);  
            }
            discordUser = new DiscordUser
            {
                Id = discordUser!.Id,
                Avatar = media.Path,
                Discriminator = discordUser.Discriminator,
                Username = discordUser.Username,
            };

            return discordUser!;
        }
    }
}