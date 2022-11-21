using RaidRobot.Data.Entities;
using RaidRobot.Infrastructure;
using RaidRobot.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public class SplitCommandInterpreter : ISplitCommandInterpreter
    {
        private readonly IRaidSplitConfiguration config;
        private readonly ITextCommunicator communicator;
        private readonly ISplitOrchestrator splitOrchestrator;
        private readonly IEventOrchestrator eventOrchestrator;

        public SplitCommandInterpreter(
            IRaidSplitConfiguration config,
            ITextCommunicator communicator,
            ISplitOrchestrator splitOrchestrator,
            IEventOrchestrator eventOrchestrator)
        {
            this.config = config;
            this.communicator = communicator;
            this.splitOrchestrator = splitOrchestrator;
            this.eventOrchestrator = eventOrchestrator;
        }

        public async Task ExecuteCommand(RaidEvent raidEvent, Split split, string content, ulong channelID)
        {
            var inputs = content.Split(" ");
            if (inputs.Length < 1)
                return;

            var numberOfSplits = raidEvent.Splits.Count;
            string message = string.Empty;
            switch (inputs[0].ToLower())
            {
                case "leader":
                    if (inputs.Length < 2)
                    {
                        message = "Proper Format 'Leader Jinoy'";
                        break;
                    }
                    message = splitOrchestrator.UpdateRole(raidEvent, raidEvent.Splits, split, inputs[1], RaidResponsibilities.Leader);
                    break;
                case "looter":
                    if (inputs.Length < 2)
                    {
                        message = "Proper Format 'Looter Jinoy'";
                        break;
                    }
                    message = splitOrchestrator.UpdateRole(raidEvent, raidEvent.Splits, split, inputs[1], RaidResponsibilities.Looter);
                    break;
                case "inviter":
                    if (inputs.Length < 2)
                    {
                        message = "Proper Format 'Inviter Jinoy'";
                        break;
                    }
                    message = splitOrchestrator.UpdateRole(raidEvent, raidEvent.Splits, split, inputs[1], RaidResponsibilities.Inviter);
                    break;
                case "move":
                    if (inputs.Length < 2)
                    {
                        message = "Proper Format 'MoveTo Jinoy'";
                        break;
                    }
                    message = splitOrchestrator.MoveTo(raidEvent, raidEvent.Splits, inputs[1], split.SplitNumber);
                    break;
                case "swap":
                    if (inputs.Length < 3)
                    {
                        message = "Proper Format 'Swap Jinoy Lilmeech'";
                        break;
                    }
                    message = splitOrchestrator.Swap(raidEvent, raidEvent.Splits, inputs[1], inputs[2]);
                    break;
                case "redo":
                    if (inputs.Length > 1 && !int.TryParse(inputs[1], out var parse))
                    {
                        numberOfSplits = parse;
                    }
                    await eventOrchestrator.PrepareSplits(raidEvent, numberOfSplits);
                    break;
                case "ignorebuddies":
                    await eventOrchestrator.PrepareSplits(raidEvent, numberOfSplits, null, true);
                    break;
                default:
                    message = "I don't understand, available commands are: Leader, Looter, Inviter, MoveTo, Swap, Redo, IgnoreBuddies";
                    break;
            }


            if (!string.IsNullOrEmpty(message))
            {
                message = $"```yaml{Environment.NewLine}{message}{Environment.NewLine}```";
                await communicator.SendMessageByChannelName(raidEvent.GuildID, config.Settings.AdminChannel, message);
            }

            foreach (var s in raidEvent.Splits.Values)
            {
                await eventOrchestrator.UpdateSplitAnnouncement(raidEvent, s);
            }
        }
    }
}
