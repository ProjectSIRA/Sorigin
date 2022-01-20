using Microsoft.EntityFrameworkCore;
using NodaTime;
using System.Text.Json.Serialization;

namespace Sorigin.Models;


[Index(nameof(FileHash))]
[Index(nameof(Contract))]
public class Media
{
    public Guid ID { get; set; }
    public long FileSize { get; set; }
    public string Path { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public string FileHash { get; set; } = null!;
    public string Contract { get; set; } = null!;

    [JsonIgnore]
    public Instant Uploaded { get; set; }
}