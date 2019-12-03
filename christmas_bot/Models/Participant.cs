using System;
using System.Text.Json.Serialization;

namespace christmas_bot.Models
{
    public class Participant
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("imageUrl")]
        public Uri ImageUrl { get; set; }
    }
}