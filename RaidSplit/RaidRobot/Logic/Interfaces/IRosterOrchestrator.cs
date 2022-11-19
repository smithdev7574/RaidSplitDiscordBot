using RaidRobot.Data.Entities;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public interface IRosterOrchestrator
    {
        Task<GuildMember> AutoMapCharacter(RaidEvent raidEvent, ulong userID, CharacterType characterType);
        GuildMember FindMember(ulong userID, CharacterType characterType);
        Task<GuildMember> MapUser(ulong guildID, ulong userID, string characterName, CharacterType characterType);
        Task<GuildMember> ValidateUser(RaidEvent raidEvent, ulong userID, CharacterType characterType);
    }
}