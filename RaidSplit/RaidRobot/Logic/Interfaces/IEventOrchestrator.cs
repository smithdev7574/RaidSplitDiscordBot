using RaidRobot.Data.Entities;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public interface IEventOrchestrator
    {
        Task AddRegistrant(RaidEvent raidEvent, CharacterType characterType, GuildMember member, ulong userID);
        Task RemoveRegistrant(RaidEvent raidEvent, CharacterType characterType, ulong userID);
        Task RemoveUser(RaidEvent raidEvent, ulong userID, CharacterType characterType);
        Task IsNowLate(RaidEvent raidEvent, ulong userID);
        Task NoLongerLate(RaidEvent raidEvent, ulong userID);
        Task WhereAmI(RaidEvent raidEvent, ulong userID);

        Task UpdateAteendeeMessage(RaidEvent raidEvent);
    }
}