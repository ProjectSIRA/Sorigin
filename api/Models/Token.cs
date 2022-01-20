using System.Text.Json.Serialization;

namespace Sorigin.Models;

public class Token
{
    [JsonPropertyName("token")]
    public string Value { get; } = null!;

    public Token(string value) => Value = value;
}