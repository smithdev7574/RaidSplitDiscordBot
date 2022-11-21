using RaidRobot.Data.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaidRobot.Data
{
    public interface ISplitDataStore
    {
        ConcurrentDictionary<Guid, RaidEvent> Events { get; }
        ConcurrentDictionary<string, GuildMember> Roster { get; }
        ConcurrentDictionary<ulong, UnknownMessage> UnknownMessages { get; }
        ConcurrentDictionary<string, PreSplit> PreSplits { get; }
        string UpdateRoster(List<GuildMember> roster);

        void SaveChanges();
    }
}