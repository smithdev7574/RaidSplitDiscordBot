using Discord;
using RaidRobot.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public interface IEventOrchestrator
    {
        Task AddRegistrant(RaidEvent raidEvent, CharacterType characterType, GuildMember member, ulong userID);
        Task RemoveRegistrant(RaidEvent raidEvent, CharacterType characterType, ulong userID);
        Task RemoveUser(RaidEvent raidEvent, ulong userID, CharacterType characterType);
        Task PrepareSplits(ulong guildID, string eventName, int numberOfSplits);
        Task PrepareSplits(RaidEvent raidEvent, int numberOfSplits, Dictionary<string, SplitAttendee> members = null, bool ignoreBuddies = false);
        Task FinalizeSplits(ulong guildID, string eventName);
        Task IsNowLate(RaidEvent raidEvent, ulong userID);
        Task NoLongerLate(RaidEvent raidEvent, ulong userID);
        Task WhereAmI(RaidEvent raidEvent, ulong userID);
        Task UpdateSplitAnnouncement(RaidEvent raidEvent, Split split);
        Task UpdateAteendeeMessage(RaidEvent raidEvent);
        Task MoveTo(ulong guildID, string eventName, string characterName, int splitNumber);
        Task Swap(ulong guildID, string eventName, string characterName1, string characterName2);
        Task OpenEvent(ulong guildID, string eventName);
        Task NeedsItem(ulong guildID, string eventName, string itemName, string characterNames);
        Task CancelEvent(ulong guildID, string eventName);
        Task RemoveCharacter(ulong guildID, string eventName, string characterName);
        Task<string> SplitASplit(ulong guildID, ulong userID, string username, string eventName, int splitNumber, int numberOfSplits);
    }
}