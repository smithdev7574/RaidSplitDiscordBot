using Discord.Commands;
using Discord.WebSocket;
using RaidRobot.Logic;
using RaidRobot.Messaging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RaidRobot.Messaging
{
    public class UploadMonitor : IUploadMonitor
    {
        private readonly DiscordSocketClient client;
        private readonly IRosterParser rosterParser;

        private bool isAwaitingUpload;
        private string userName;
        private ulong userID;
        private ulong channelID;
        private ulong guildID;


        public UploadMonitor(
            DiscordSocketClient client,
            IRosterParser rosterParser)
        {
            this.client = client;
            this.rosterParser = rosterParser;
        }

        public string MonitorRosterUpload(SocketCommandContext context)
        {
            if (isAwaitingUpload)
                return $"I am sorry {userName} is already updating the roster.";

            isAwaitingUpload = true;
            userID = context.User.Id;
            userName = context.User.Username;
            channelID = context.Channel.Id;
            guildID = context.Guild.Id;


            var channel = context.Channel.Name;
            var message = $"Please post the roster to the {channel} channel within 5 minutes.";

            var waitTask = awaitUpload();

            return message;
        }


        private async Task awaitUpload()
        {
            var expirationDT = DateTime.Now.AddMinutes(5);
            client.MessageReceived += Client_MessageReceived;

            while (isAwaitingUpload && DateTime.Now <= expirationDT)
            {
                await Task.Delay(500);
            }
            client.MessageReceived -= Client_MessageReceived;
            isAwaitingUpload = false;
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            try
            {
                if (arg.Author.Id != userID)
                    return;

                if (arg.Channel.Id != channelID)
                    return;

                if (!arg.Attachments.Any())
                    return;

                var attachment = arg.Attachments.First();
                var parseTask = rosterParser.ParseRoster(attachment.Url, guildID, channelID);

                isAwaitingUpload = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Parsing File {ex.Message}");
            }
        }
    }
}
