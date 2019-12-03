using System;
using System.Net.Mail;
using System.Text.Json.Serialization;

namespace christmas_bot.Models
{
    public class Participant
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public MailAddress Email { get; set; }

        [JsonPropertyName("imageUrl")]
        public Uri ImageUrl { get; set; }
    }
}