using System.Text.Json.Serialization;

namespace christmas_bot.Models
{
    public class PreMatch
    {
        [JsonPropertyName("from")]
        public string FromEmail { get; set; }

        [JsonPropertyName("to")]
        public string ToEmail { get; set; }
    }
}