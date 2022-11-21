using RaidRobot.Data.Entities;
using RaidRobot.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public interface IRaidSplitter
    {
        AttendeeResponse RemoveFromSplit(Dictionary<int, Split> splits, SplitAttendee splitAttendee);
        AttendeeResponse RemoveFromSplit(Dictionary<int, Split> splits, ulong userId, CharacterType characterType);
        AttendeeResponse AddToSplit(RaidEvent raidEvent, Dictionary<int, Split> splits, ulong userId, CharacterType characterType, bool ignoreBuddies = false);
        AttendeeResponse AddToSplit(RaidEvent raidEvent, Dictionary<int, Split> splits, SplitAttendee attendee, bool ignoreBuddies = false);
        Task<Dictionary<int, Split>> Split(RaidEvent raidEvent, int numberOfSplits, Dictionary<string, SplitAttendee> members = null, bool ignoreBuddies = false);
    }
}