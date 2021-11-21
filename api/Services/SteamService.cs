using Sorigin.Models.Platforms;
using Sorigin.Settings;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sorigin.Services;

public interface ISteamService
{
    Task<SteamUser?> GetProfile(string ticket);
    Task<SteamUser?> GetProfileFromID(string steamID);
}

internal class SteamService : ISteamService
{
    private readonly ILogger _logger;
    private readonly HttpClient _client;
    private readonly ISteamSettings _steamSettings;

    public SteamService(ILogger<SteamService> logger, HttpClient client, ISteamSettings steamSettings)
    {
        _logger = logger;
        _client = client;
        _steamSettings = steamSettings;
    }
    public async Task<SteamUser?> GetProfile(string ticket)
    {
        _logger.LogDebug("Getting active user profile.");
        HttpResponseMessage response = await _client.GetAsync($"https://api.steampowered.com/ISteamUserAuth/AuthenticateUserTicket/v1?key={_steamSettings.Key}&appid={_steamSettings.AppID}&ticket={ticket}");
        if (response.IsSuccessStatusCode)
        {
            SteamResponse<SteamResult> steamResult = (await JsonSerializer.DeserializeAsync<SteamResponse<SteamResult>>(await response.Content.ReadAsStreamAsync()))!;
            return await GetProfileFromID(steamResult.Response.Params!.SteamID);
        }
        _logger.LogError("Could not authenticate user from the steam API.");
        return null;
    }

    public async Task<SteamUser?> GetProfileFromID(string steamID)
    {
        _logger.LogDebug("Getting user profile ({steamID})", steamID);
        HttpResponseMessage response = await _client.GetAsync($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={_steamSettings.Key}&steamids={steamID}");
        if (response.IsSuccessStatusCode)
        {
            SteamResponse<SteamPlayerSummaries> steamSummaryResponse = (await JsonSerializer.DeserializeAsync<SteamResponse<SteamPlayerSummaries>>(await response.Content.ReadAsStreamAsync()))!;
            if (steamSummaryResponse.Response.Players.Length == 0)
                return null;
            var player = steamSummaryResponse.Response.Players[0];
            _logger.LogDebug("User Profile {PersonaName} ({SteamID})", player.PersonaName, player.SteamID);
            return new SteamUser { Id = player.SteamID, Username = player.PersonaName, Avatar = player.AvatarHash };
        }
        return null;
    }

    public class SteamResponse<T> where T : notnull
    {
        [JsonPropertyName("response")]
        public T Response { get; set; } = default!;
    }
}