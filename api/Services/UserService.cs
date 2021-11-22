using Microsoft.EntityFrameworkCore;
using NodaTime;
using Sorigin.Models;

namespace Sorigin.Services;

public interface IUserService
{
    Task<User?> GetUser(ulong id);
    Task<User> CreateUser(ulong id, string username, string pfpContract, Uri profilePicture, string? country = null);
    Task UpdateUser(User user, string? username = null, string? pfpContract = null, Uri? profilePicture = null, string? country = null);
    Task Login(User user);
}

internal class UserService : IUserService
{
    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly IMediaService _mediaService;
    private readonly SoriginContext _soriginContext;

    public UserService(IClock clock, ILogger logger, HttpClient httpClient, IMediaService mediaService, SoriginContext soriginContext)
    {
        _clock = clock;
        _logger = logger;
        _httpClient = httpClient;
        _mediaService = mediaService;
        _soriginContext = soriginContext;
    }

    public async Task<User> CreateUser(ulong id, string username, string pfpContract, Uri profilePicture, string? country = null)
    {
        _logger.LogInformation("Initializing the user creation process.");

        _logger.LogInformation("Creating a new user account for {Username} ({ID})", username, id);
        User? user = await GetUser(id);
        if (user is not null)
        {
            _logger.LogWarning("The user {ID} already exists! Cannot create the user. Returning the active user.", id);
            return user;
        }

        user = new()
        {
            ID = id,
            Country = country,
            Username = username,
            LastLogin = _clock.GetCurrentInstant(),
            Registration = _clock.GetCurrentInstant(),
            ProfilePictureMedia = await GetProfilePicture(id, pfpContract, profilePicture),
        };

        _soriginContext.Users.Add(user);
        await _soriginContext.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUser(ulong id)
    {
        User? user = await _soriginContext.Users.Include(u => u.ProfilePictureMedia).FirstOrDefaultAsync(u => u.ID == id);
        return user;
    }

    public async Task Login(User user)
    {
        user.LastLogin = _clock.GetCurrentInstant();
        await _soriginContext.SaveChangesAsync();
    }

    public async Task UpdateUser(User user, string? username = null, string? pfpContract = null, Uri? profilePicture = null, string? country = null)
    {
        if (user.Username is null && pfpContract is null && pfpContract is null && country is null)
            return;

        bool didUpdate = false;

        if (username != null && username != user.Username)
        {
            user.Username = username;
            didUpdate = true;
        }
        if (pfpContract != null && profilePicture != null && pfpContract != user.ProfilePictureMedia.Contract)
        {
            Media? newMedia = await GetProfilePicture(user.ID, pfpContract, profilePicture);
            if (newMedia != null && newMedia.Contract != "fallback")
            {
                user.ProfilePictureMedia = newMedia;
                didUpdate = true;
            }
        }
        if (country != null && country != user.Country)
        {
            user.Country = country;
            didUpdate = true;
        }

        if (didUpdate)
            await _soriginContext.SaveChangesAsync();
    }

    private async Task<Media> GetProfilePicture(ulong id, string contract, Uri profilePicture)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(profilePicture);
        if (response.IsSuccessStatusCode)
        {
            using Stream stream = await response.Content.ReadAsStreamAsync();
            using MemoryStream ms = new();
            await stream.CopyToAsync(ms);

            Media media = await _mediaService.Upload("avatars", id.ToString(), Path.GetFileName(profilePicture.ToString()), ms, contract);
            return media;
        }

        // If we couldn't get the profile picture, we use a default
        return await _soriginContext.Media.FirstAsync(m => m.Contract == "fallback");
    }
}