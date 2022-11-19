using RaidRobot.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Models
{
    public class AttendeeResponse
    {

        public bool HasError { get; set; }
        public string Message { get; set; }
        public Split Split { get; set; }
        public SplitAttendee Attendee { get; set; }
    }
}
