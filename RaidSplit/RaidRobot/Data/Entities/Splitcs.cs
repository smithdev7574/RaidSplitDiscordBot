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
        public SplitMember Leader { get; set; }
        public SplitMember MasterLooter { get; set; }
        public SplitMember Inviter { get; set; }

        public List<(DateTime actionTime, string action)> Actions = new List<(DateTime actionTime, string action)>();

        public Dictionary<MessageContexts, MessageDetail> Messages = new Dictionary<MessageContexts, MessageDetail>();
        public Dictionary<string, SplitMember> Members { get; set; } = new Dictionary<string, SplitMember>();

        public decimal ClassWeight(string className)
        {
            var weight = this.Members.Values.Where(x => x.ClassName.ToLower() == className.ToLower()).Sum(x => x.Weight);
            return weight;
        }

        public decimal ClassWeightWithoutBoxes(string className)
        {
            var weight = this.Members.Values.Where(x => x.ClassName.ToLower() == className.ToLower() && !x.IsBox).Sum(x => x.Weight);
            return weight;
        }
    }
}
