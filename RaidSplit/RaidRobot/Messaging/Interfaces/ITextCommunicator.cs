using Discord.Rest;
using System.Threading.Tasks;

namespace RaidRobot.Messaging
{
    public interface ITextCommunicator
    {
        Task DeleteMessage(ulong guildID, ulong channelID, ulong messageID);
        Task React(ulong guildID, ulong channelID, ulong messageID, string emojiString);
        Task<RestUserMessage> SendMessage(ulong guildID, ulong channelID, string message);
        Task<RestUserMessage> SendMessageByChannelName(ulong guildID, string channelName, string message);
        Task SendTell(ulong userID, string message);
        Task UpdateMessage(ulong guildID, ulong channelID, ulong messageID, string content);
    }
}