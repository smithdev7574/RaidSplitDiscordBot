using Discord.WebSocket;
using RaidRobot.Data;
using RaidRobot.Data.Entities;
using RaidRobot.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Messaging
{
    public class MessageMonitor : IMessageMonitor
    {
        private readonly DiscordSocketClient client;
        private readonly ISplitDataStore splitDataStore;
        private readonly IRosterOrchestrator rosterOrchestrator;
        private readonly ISplitCommandInterpreter commandInterpreter;

        public MessageMonitor(
            DiscordSocketClient client,
            ISplitDataStore splitDataStore,
            IRosterOrchestrator rosterOrchestrator,
            ISplitCommandInterpreter commandInterpreter)
        {
            this.client = client;
            this.splitDataStore = splitDataStore;
            this.rosterOrchestrator = rosterOrchestrator;
            this.commandInterpreter = commandInterpreter;
        }

        public void Initialize()
        {
            client.MessageReceived += (message) => { return messageRecieved(message); };
        }

        private async Task messageRecieved(SocketMessage message)
        {
            try
            {
                //We don't care about messages that aren't a reply.
                if (message.Reference == null || !message.Reference.MessageId.IsSpecified)
                    return;

                if (splitDataStore.UnknownMessages.ContainsKey(message.Reference.MessageId.Value))
                {
                    //Not awaiting on purpose...
                    var task = mapUser(message, message.Reference.MessageId.Value);
                }
                else
                {
                    var matchingEvent = findSplitByMessage(message.Reference.MessageId.Value);
                    if (matchingEvent.context == null)
                        return;

                    if (matchingEvent.context == MessageContexts.ResponsibilityMessage)
                    {
                        var content = message.Content;
                        //Not Awaiting on Purpose...
                        var task = commandInterpreter.ExecuteCommand(matchingEvent.raidEvent, matchingEvent.split, content, message.Channel.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling message. {ex.Message}");
            }
        }

        private async Task mapUser(SocketMessage message, ulong replyingToMessageID)
        {
            var inputs = message.Content.Split(" ");
            if (inputs.Length < 1)
                return;


            var unknownMessage = splitDataStore.UnknownMessages[replyingToMessageID];
            switch (unknownMessage.MessageType)
            {
                case UnknownMessageTypes.Character:
                    await rosterOrchestrator.MapUser(unknownMessage, message.Content);
                    break;
                case UnknownMessageTypes.Class:
                    await rosterOrchestrator.UpdateClass(unknownMessage, message.Content);
                    break;
                default:
                    break;
            }
        }

        private (RaidEvent raidEvent, Split split, MessageContexts? context) findSplitByMessage(ulong messageID)
        {
            foreach (var raidEvent in splitDataStore.Events.Values.ToList())
            {
                foreach (var split in raidEvent.Splits.Values.ToList())
                {
                    foreach (var message in split.Messages.ToList())
                    {
                        if (message.Value.MessageID == messageID)
                        {
                            return (raidEvent, split, message.Key);
                        }
                    }
                }
            }
            return (null, null, null);
        }





    }
}
