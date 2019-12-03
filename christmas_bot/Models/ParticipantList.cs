using System.Collections.Generic;
using System.Linq;

namespace christmas_bot.Models
{
    public class ParticipantList : List<Participant>
    {
        public Participant GetParticipantByEmail(string email)
        {
            return this.First(p => p.Email == email);
        }
    }
}