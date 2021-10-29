using System.Text.Json.Serialization;

namespace Sorigin
{
    public class Error
    {
        [JsonPropertyName("error")]
        public string? ErrorMessage { get; set; }

        public Error(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public static object Create(string message)
        {
            string error = string.IsNullOrEmpty(message) ? "An unknown error has occured." : message;
            return new { error };
        }
    }
}