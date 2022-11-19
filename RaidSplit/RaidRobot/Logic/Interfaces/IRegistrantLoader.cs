using Discord;
using RaidRobot.Data.Entities;
using RaidRobot.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public interface IRegistrantLoader
    {
        Task<List<ulong>> GetLateRegistrants(RaidEvent raidEvent);
        Task<List<(CharacterType CharacterType, IUser User)>> GetReactions(RaidEvent raidEvent);
        Task<RegistrantResponse> GetRegistrants(RaidEvent raidEvent);
    }
}