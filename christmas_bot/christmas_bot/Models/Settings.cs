using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace christmas_bot.Models
{
    public class Settings
    {
        public Settings()
        { }

        public static Settings Load(string filepath)
        {
            string text = File.ReadAllText(filepath);
            var settings = JsonSerializer.Deserialize<Settings>(text);
            settings.Validate();
            return settings;
        }

        [JsonPropertyName("participants")]
        public ParticipantList Participants { get; set; }

        [JsonPropertyName("preMatches")]
        public IList<(MailAddress fromEmail, MailAddress toEmail)> PreMatches { get; set; }

        [JsonPropertyName("badMatchGroups")]
        public IList<IList<MailAddress>> BadMatchGroups { get; set; }

        public void Validate()
        {
            var errors = new StringBuilder();
            if (this.Participants == null || !this.Participants.Any())
            {
                errors.AppendLine("Participants undefined");
            }
            else if (this.Participants.Any(p => string.IsNullOrEmpty(p.Name) || p.Email == null || p.ImageUrl == null))
            {
                errors.AppendLine("All participants must have a name, email, and imageUrl defined");
            }
            else
            {
                if (this.Participants.Count != this.Participants.Select(p => p.Email).Distinct().Count())
                {
                    errors.AppendLine("All participants must have a unique email");
                }
                if (this.Participants.Count != this.Participants.Select(p => p.ImageUrl).Distinct().Count())
                {
                    errors.AppendLine("All participants must have a unique image url");
                }
            }

            var participantEmails = this.Participants.Select(p => p.Email).ToArray();

            if (this.PreMatches != null && this.PreMatches.Any())
            {
                if (this.PreMatches.Any(pm => pm.fromEmail == null || pm.toEmail == null))
                {
                    errors.AppendLine("All prematches must have a from: and to:");
                }
                else
                {
                    if (this.PreMatches.Count != this.PreMatches.Select(p => p.fromEmail).Distinct().Count()
                        || this.PreMatches.Count != this.PreMatches.Select(p => p.toEmail).Distinct().Count())
                    {
                        errors.AppendLine("Invalid prematch rules - make sure people are only defined once");
                    }
                    if (this.PreMatches.SelectMany(pm => new[] { pm.fromEmail, pm.toEmail }).Any(e => !participantEmails.Contains(e)))
                    {
                        errors.AppendLine("All prematch emails must appear in the participants list");
                    }
                }
            }

            if (this.BadMatchGroups != null && this.BadMatchGroups.Any())
            {
                if (this.BadMatchGroups.Any(bm => bm == null || !bm.Any() || bm.Any(x => x == null)))
                {
                    errors.AppendLine("Don't specify empty bad match groups");
                }
                else if (this.BadMatchGroups.SelectMany(bm => bm).Any(e => !participantEmails.Contains(e)))
                {
                    errors.AppendLine("All bad match emails must appear in the participants list");
                }
            }

            if (errors.Length > 0)
            {
                throw new Exception(errors.ToString());
            }
        }
    }
}