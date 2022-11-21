using RaidRobot.Data.Entities;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public interface ISplitCommandInterpreter
    {
        Task ExecuteCommand(RaidEvent raidEvent, Split split, string content, ulong channelID);
    }
}