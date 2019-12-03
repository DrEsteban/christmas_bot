using System.Text.Json.Serialization;

namespace christmas_bot.Models
{
    public class PreMatch
    {
        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }
    }
}