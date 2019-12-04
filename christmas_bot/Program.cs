using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using christmas_bot.Models;
using FluentEmail.Core;
using FluentEmail.Smtp;

namespace christmas_bot
{
    internal class Program
    {
        private static IList<Match> _matches = new List<Match>();
        private static ParticipantList _participants;
        private static Settings _settings;

        private static void Main(string[] args)
        {
            if (args == null || !args.Any())
            {
                Console.Error.WriteLine("Specify a .json settings file path as the first argument.");
                return;
            }

            string smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST")?.Trim();
            string smtpUser = Environment.GetEnvironmentVariable("SMTP_USER")?.Trim();
            string smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS")?.Trim();
            string wishListLink = Environment.GetEnvironmentVariable("WISHLIST_LINK")?.Trim();
            if (string.IsNullOrEmpty(smtpHost)
                || string.IsNullOrEmpty(smtpUser)
                || string.IsNullOrEmpty(smtpPass)
                || string.IsNullOrEmpty(wishListLink))
            {
                Console.Error.WriteLine("Please specify the following environment variables: SMTP_HOST, SMTP_USER, SMTP_PASS, WISHLIST_LINK");
                return;
            }

            try
            {
                _settings = Settings.Load(args[0]);
                _participants = _settings.Participants;

                // Main selection loop
                while (true)
                {
                    // Pre-matches
                    var preSelectedGivers = new List<Participant>();
                    if (_settings.PreMatches != null)
                    {
                        foreach (var match in _settings.PreMatches)
                        {
                            var from = _participants.GetParticipantByEmail(match.FromEmail);
                            var to = _participants.GetParticipantByEmail(match.ToEmail);
                            _matches.Add(new Match(from, to));
                            preSelectedGivers.Add(from);
                        }
                    }

                    // Random matches
                    var r = new Random();
                    foreach (var from in _participants.Except(preSelectedGivers))
                    {
                        var candidates = GetCandidatesForParticipant(from);
                        if (!candidates.Any())
                        {
                            // Bad run, reset and try again
                            Console.WriteLine("Bad run, trying again...");
                            _matches.Clear();
                            continue;
                        }
                        var to = candidates[r.Next(0, candidates.Count)];  // Choose a receiver at random
                        _matches.Add(new Match(from, to));
                    }

                    // Final check
                    if (_matches.Count != _participants.Count)
                    {
                        Console.Error.WriteLine("Something really weird happened, trying again...");
                        _matches.Clear();
                        continue;
                    }
                    break;
                }

                Console.WriteLine("Matches selected! Sending emails...");
                Console.WriteLine();

                // Send mail
                var smtpClient = new SmtpClient(smtpHost)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };
                Email.DefaultSender = new SmtpSender(smtpClient);
                foreach (var match in _matches)
                {
                    Console.WriteLine($"Sending email to {match.From.Email}...");
                    var email = Email.From(smtpUser)
                                     .To(match.From.Email)
                                     .Subject($"Hey {match.From.Name}! Your secret santa has been chosen.")
                                     .Body($"Your secret santa recipient this year is <b>{match.To.Name}</b>!<br><br><img src=\"{match.To.ImageUrl}\" alt=\"{match.To.Name}\"/><br><br>To see their wishlist and to fill out your own, go here!: <a href=\"{wishListLink}\" _target=\"blank\">{wishListLink}</a>", isHtml: true)
                                     .Send();

                    if (email.Successful)
                    {
                        Console.WriteLine($"Sent email to {match.From.Email}.");
                    }
                    else
                    {
                        Console.Error.WriteLine($"Failed to send to {match.From.Email}. Errors: {string.Join(", ", email.ErrorMessages)}");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Done!");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{e.GetType().Name}: {e.Message}");
            }
        }

        /// <summary>
        /// Gets valid gift candidates for this participant
        /// </summary>
        private static IList<Participant> GetCandidatesForParticipant(Participant from)
        {
            var candidates = new List<Participant>(_participants);
            candidates.Remove(from);

            var alreadyReceiving = _matches.Select(m => m.To).ToArray();
            foreach (var p in _participants.Where(p => alreadyReceiving.Contains(p)))
            {
                candidates.Remove(p);
            }

            if (_settings.BadMatchGroups != null)
            {
                foreach (var group in _settings.BadMatchGroups)
                {
                    if (group.Contains(from.Email))
                    {
                        foreach (var exclusion in group)
                        {
                            candidates.Remove(_participants.GetParticipantByEmail(exclusion));
                        }
                    }
                }
            }

            return candidates;
        }
    }
}