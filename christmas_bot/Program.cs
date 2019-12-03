using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using christmas_bot.Models;

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

            try
            {
                _settings = Settings.Load(args[0]);
                _participants = _settings.Participants;

                var alreadyGiving = AddPreMatches();

                var r = new Random();
                foreach (var from in _participants.Except(alreadyGiving))
                {
                    var candidates = GetCandidatesForParticipant(from);
                    var to = candidates[r.Next(0, candidates.Count)];  // Choose a receiver at random
                    _matches.Add(new Match(from, to));
                }

                Debug.Assert(_matches.Count == _participants.Count);

                // TODO Send mail results
                Console.WriteLine(JsonSerializer.Serialize(_matches, new JsonSerializerOptions() { WriteIndented = true }));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{e.GetType().Name}: {e.Message}");
            }
        }

        /// <summary>
        /// Adds prematches
        /// </summary>
        /// <returns>A list of people given a match</returns>
        private static IList<Participant> AddPreMatches()
        {
            var result = new List<Participant>();
            if (_settings.PreMatches != null)
            {
                foreach (var match in _settings.PreMatches)
                {
                    var from = _participants.GetParticipantByEmail(match.From);
                    var to = _participants.GetParticipantByEmail(match.To);
                    _matches.Add(new Match(from, to));
                    result.Add(from);
                }
            }

            return result;
        }

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