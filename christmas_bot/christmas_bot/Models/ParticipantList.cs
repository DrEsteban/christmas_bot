using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace christmas_bot.Models
{
    public class ParticipantList : List<Participant>
    {
        public Participant GetParticipantByEmail(MailAddress email)
        {
            return this.First(p => p.Email == email);
        }
    }
}