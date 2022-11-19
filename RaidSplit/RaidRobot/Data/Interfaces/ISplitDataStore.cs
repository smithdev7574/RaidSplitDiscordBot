using RaidRobot.Data.Entities;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RaidRobot.Data
{
    public interface ISplitDataStore
    {
        ConcurrentDictionary<Guid, RaidEvent> Events { get; }
        ConcurrentDictionary<string, GuildMember> Roster { get; }
        ConcurrentDictionary<ulong, UnknownMessage> UnknownMessages { get; }

        void SaveChanges();
    }
}