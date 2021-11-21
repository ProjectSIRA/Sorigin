namespace Sorigin.Models.Platforms;

public record SteamUser
{
    public string Id { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Avatar { get; set; } = null!;
}