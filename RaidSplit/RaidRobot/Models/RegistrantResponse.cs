using RaidRobot.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Models
{
    public class RegistrantResponse
    {
        public Dictionary<string, SplitAttendee> Members { get; set; } = new Dictionary<string, SplitAttendee>();
        public List<UnknownUser> UnknownUsers { get; set; } = new List<UnknownUser>();
        public List<SplitAttendee> IncompleteUsers { get; set; } = new List<SplitAttendee>();
    }

}
