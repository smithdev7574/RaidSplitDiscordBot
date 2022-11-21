using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Data.Entities
{
    public class Split
    {
        public int SplitNumber { get; set; }
        public SplitAttendee Leader { get; set; }
        public SplitAttendee MasterLooter { get; set; }
        public SplitAttendee Inviter { get; set; }

        public List<(DateTime actionTime, string action)> Actions = new List<(DateTime actionTime, string action)>();

        public Dictionary<MessageContexts, MessageDetail> Messages = new Dictionary<MessageContexts, MessageDetail>();
        public Dictionary<string, SplitAttendee> Attendees { get; set; } = new Dictionary<string, SplitAttendee>();

        public decimal ClassWeight(string className)
        {
            var weight = this.Attendees.Values.Where(x => x.ClassName.ToLower() == className.ToLower()).Sum(x => x.Weight);
            return weight;
        }

        public decimal ClassWeightWithoutBoxes(string className)
        {
            var weight = this.Attendees.Values.Where(x => x.ClassName.ToLower() == className.ToLower() && !x.IsBox).Sum(x => x.Weight);
            return weight;
        }
    }
}
