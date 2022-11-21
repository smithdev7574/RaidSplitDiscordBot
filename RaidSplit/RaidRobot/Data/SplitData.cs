using RaidRobot.Data.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Data
{
    public class SplitData
    {
        public ConcurrentDictionary<string, GuildMember> Roster { get; set; } = new ConcurrentDictionary<string, GuildMember>();
        public ConcurrentDictionary<Guid, RaidEvent> Events { get; set; } = new ConcurrentDictionary<Guid, RaidEvent>();
        public ConcurrentDictionary<ulong, UnknownMessage> UnknownMessages { get; set; } = new ConcurrentDictionary<ulong, UnknownMessage>();
        public ConcurrentDictionary<string, PreSplit> PreSplits { get; set; } = new ConcurrentDictionary<string, PreSplit>();
    }
}
