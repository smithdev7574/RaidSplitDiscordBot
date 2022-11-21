using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public interface IRosterParser
    {
        Task ParseRoster(string attachmentUrl, ulong guildID, ulong channelID);
    }
}