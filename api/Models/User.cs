using Microsoft.EntityFrameworkCore;
using NodaTime;
using System.Text.Json.Serialization;

namespace Sorigin.Models;

[Index(nameof(ID))]
[Index(nameof(Username))]
public class User
{
    public ulong ID { get; set; }
    public string Username { get; set; } = null!;
    public string? Country { get; set; } = null!;

    public string ProfilePicture => ProfilePictureMedia.Path;

    [JsonIgnore]
    public Media ProfilePictureMedia { get; set; } = null!;

    public Instant Registration { get; set; }
    public Instant LastLogin { get; set; }
}