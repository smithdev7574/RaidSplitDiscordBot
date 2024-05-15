using Discord.Rest;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Messaging
{
    public class TextCommunicator : ITextCommunicator
    {
        private readonly DiscordSocketClient client;

        public TextCommunicator(DiscordSocketClient client)
        {
            this.client = client;
        }

        public async Task<RestUserMessage> SendMessage(ulong guildID, ulong channelID, string message)
        {
            var guild = client.GetGuild(guildID);
            var channel = guild?.GetTextChannel(channelID);

            if (channel == null)
                throw new Exception($"Could not locate channel id {channelID} in guild {guildID}");

            var returnMessage = await channel.SendMessageAsync(message);
            return returnMessage;
        }

        public async Task<RestUserMessage> SendMessageByChannelName(ulong guildID, string channelName, string message, ComponentBuilder componentBuilder = null)
        {
            var guild = client.GetGuild(guildID);
            var channel = guild.TextChannels.FirstOrDefault(x => x.Name == channelName);

            if (channel == null)
                throw new Exception($"Could not locate channel {channelName} in guild {guildID}");


            RestUserMessage returnMessage = null;
            if (componentBuilder != null)
                try
                {
                    returnMessage = await channel.SendMessageAsync(message, components: componentBuilder.Build());
                }
                catch(Exception ex)
                {
                    returnMessage = await channel.SendMessageAsync(message);
                }
            else
                returnMessage = await channel.SendMessageAsync(message);

            return returnMessage;
        }

        public async Task SendTell(ulong userID, string message)
        {
            var user = client.GetUser(userID);
            if (user == null)
                return;

            await user.SendMessageAsync(message);
        }

        public async Task React(ulong guildID, ulong channelID, ulong messageID, string emojiString)
        {
            var guild = client.GetGuild(guildID);
            var channel = guild?.GetTextChannel(channelID);
            var message = await channel.GetMessageAsync(messageID);

            if (message != null)
            {
                var emoji = new Emoji(emojiString);
                await message.AddReactionAsync(emoji);
            }
        }

        public async Task DeleteMessage(ulong guildID, ulong channelID, ulong messageID)
        {
            var guild = client.GetGuild(guildID);
            var channel = guild?.GetTextChannel(channelID);
            var message = await channel?.GetMessageAsync(messageID);

            if (message != null)
                await message.DeleteAsync();
        }

        public async Task UpdateMessage(ulong guildID, ulong channelID, ulong messageID, string content)
        {
            var guild = client.GetGuild(guildID);
            var channel = guild?.GetTextChannel(channelID);

            if (channel != null)
            {
                await channel.ModifyMessageAsync(messageID, msg =>
                {
                    msg.Content = content;
                });
            }
        }
    }
}
