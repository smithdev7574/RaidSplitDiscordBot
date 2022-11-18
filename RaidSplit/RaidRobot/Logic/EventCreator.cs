using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using RaidRobot.Data.Entities;
using RaidRobot.Data.Interfaces;
using RaidRobot.Infrastructure;
using RaidRobot.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public class EventCreator : IEventCreator
    {
        private readonly IRaidSplitConfiguration config;
        private readonly ISplitDataStore splitDataStore;
        private readonly ITextCommunicator textCommunicator;

        public EventCreator(
            IRaidSplitConfiguration config,
            ISplitDataStore splitDataStore,
            ITextCommunicator textCommunicator)
        {
            this.config = config;
            this.splitDataStore = splitDataStore;
            this.textCommunicator = textCommunicator;
        }

        public async Task CreateEvent(SocketCommandContext context, IGuildUser user, string eventName, DateTime raidTime, string raidTypeName)
        {
            if (user == null || string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException("User nor event name can be null");

            var eventExists = splitDataStore.Events.Values
                .Any(x => string.Equals(x.EventName, eventName, StringComparison.OrdinalIgnoreCase)
                    && x.ExpirationDT >= DateTime.Now);

            if (eventExists)
                throw new ArgumentException($"There is already an active event named {eventName}. Cancel the existing event or choose another name.");

            var channel = context.Guild.Channels.FirstOrDefault(x => x.Name == config.Settings.RegistrationChannel);
            if (channel == null)
                throw new ArgumentException($"Could not find a Registration Channel Named {config.Settings.RegistrationChannel}. Please check your appsettings.json");

            var raidType = config.RaidTypes.FirstOrDefault(x => string.Equals(x.Name, raidTypeName, StringComparison.OrdinalIgnoreCase));
            if (raidType == null)
                throw new ArgumentException($"{raidTypeName} is not a valid Raid Type. Acceptable Values: {string.Join(", ", config.RaidTypes.Select(x => x.Name))}");


            var raidEvent = new RaidEvent()
            {
                ExpirationDT = raidTime.AddHours(Constants.DEFAULT_EVENT_HOURS),
                EventName = eventName,
                UserID = user.Id,
                GuildID = context.Guild.Id,
                EventDT = raidTime,
                UserName = user.Nickname ?? user.DisplayName,
                RaidType = raidType,
            };

            var registrationMessage = BuildRegistrationMessage(raidEvent);
            var registrationMessageResult = await textCommunicator.SendMessage(context.Guild.Id, channel.Id, registrationMessage);
            raidEvent.Messages[MessageContexts.RegistrationMessage] = registrationMessageResult.ConvertToMessageDetail();

            foreach (var characterType in raidType.CharacterTypes)
            {
                await textCommunicator.React(context.Guild.Id, channel.Id, registrationMessageResult.Id, characterType.EmojiCode);
            }
            await textCommunicator.React(context.Guild.Id, channel.Id, registrationMessageResult.Id, Reactions.LateCode);
            await textCommunicator.React(context.Guild.Id, channel.Id, registrationMessageResult.Id, Reactions.NoShowCode);
            await textCommunicator.React(context.Guild.Id, channel.Id, registrationMessageResult.Id, Reactions.HelpCode);

            var attendeeMessage = BuildAttendeeMessage(raidEvent);
            var attendeeResult = await textCommunicator.SendMessage(context.Guild.Id, channel.Id, attendeeMessage);
            raidEvent.Messages[MessageContexts.AttendeeMessage] = attendeeResult.ConvertToMessageDetail();

            var announceMessage = $"{context.Guild.EveryoneRole.Mention} Sign up for {raidEvent.EventName} above.";
            var announceResult = await textCommunicator.SendMessage(context.Guild.Id, channel.Id, announceMessage);
            raidEvent.Messages[MessageContexts.EveryoneRegistrationMessage] = announceResult.ConvertToMessageDetail();

            splitDataStore.Events.TryAdd(raidEvent.EventID, raidEvent);
            splitDataStore.SaveChanges();
        }

        public string BuildRegistrationMessage(RaidEvent raidEvent)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"**New {raidEvent.RaidType.Name} Raid {raidEvent.EventName} on {raidEvent.EventDT.ToShortDateString()} at {raidEvent.EventDT.ToString("hh:mm tt")} Sign Up Below**");
            sb.AppendLine("```md");
            sb.AppendLine("1. To register for the raid react to this message using the appropriate emoji for your character type.");
            sb.AppendLine($"--- {string.Join(", ", raidEvent.RaidType.CharacterTypes.Select(x => $"{x.Name} {x.EmojiCode}"))}");
            sb.AppendLine("2. If you are bringing multiple characters, please react to all the emojis that apply.");
            sb.AppendLine("3. Use the clock emoji if you are going to be late. Uncheck it when you arrive to be put into a split.");
            sb.AppendLine("4. Use the no entry emoji if you cannot make the raid.");
            sb.AppendLine("5. Use question mark emoji once the splits are announced to easily find your split.");
            sb.AppendLine("```");

            return sb.ToString();
        }

        public string BuildAttendeeMessage(RaidEvent raidEvent)
        {
            return "TBD";
        }
    }
}
