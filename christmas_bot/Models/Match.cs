namespace christmas_bot.Models
{
    public class Match
    {
        public Match(Participant from, Participant to)
        {
            From = from;
            To = to;
        }

        public Participant From { get; set; }

        public Participant To { get; set; }
    }
}