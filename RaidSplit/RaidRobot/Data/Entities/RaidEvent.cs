using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Data.Entities
{
    public class RaidEvent
    {
        public Guid EventID { get; set; } = Guid.NewGuid();
        public string EventName { get; set; }
        public DateTime EventDT { get; set; }
        public DateTime ExpirationDT { get; set; }
        public ulong GuildID { get; set; }
        public ulong UserID { get; set; }
        public string UserName { get; set; }

        public RaidType RaidType { get; set; }
        public List<ItemNeed> ItemNeeds { get; set; } = new List<ItemNeed>();
        public int SubSplitCount { get; set; }

        public Dictionary<int, Split> PreparingSplits { get; set; } = new Dictionary<int, Split>();
        public Dictionary<string, SplitMember> LateMembers { get; set; } = new Dictionary<string, SplitMember>();

        public Dictionary<MessageContexts, MessageDetail> Messages { get; set; } = new Dictionary<MessageContexts, MessageDetail>();

        public DateTime? FinalizedDT { get; set; }

        public string SourceEventName { get; set; }
        public int SourceEventSplitNumber { get; set; }
        public bool IsASubSplit { get; set; }


        public override string ToString()
        {
            return $"{EventName} {EventDT}";
        }
    }
}
