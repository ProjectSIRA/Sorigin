using System.Text.Json.Serialization;

namespace Sorigin.Models
{
    public class Token
    {
        [JsonPropertyName("token")]
        public string Value { get; }

        [JsonPropertyName("refreshToken"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Refresh { get; }

        public Token(string value)
            => Value = value;

        public Token(string token, string refreshToken)
        {
            Value = token;
            Refresh = refreshToken;
        }
    }
}
