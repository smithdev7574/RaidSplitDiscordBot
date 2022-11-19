using Discord;
using Discord.WebSocket;
using RaidRobot.Data;
using RaidRobot.Data.Entities;
using RaidRobot.Infrastructure;
using RaidRobot.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public class EventOrchestrator : IEventOrchestrator
    {
        private readonly DiscordSocketClient client;
        private readonly ITextCommunicator communicator;
        private readonly IRaidSplitter raidSplitter;
        private readonly IMessageBuilder messageBuilder;
        private readonly IRegistrantLoader registrantLoader;
        private readonly IRaidSplitConfiguration config;
        private readonly ISplitDataStore splitDataStore;
        private readonly IGuildMemberConverter converter;

        public EventOrchestrator(
            DiscordSocketClient client,
            ITextCommunicator communicator,
            IRaidSplitter raidSplitter,
            IMessageBuilder messageBuilder,
            IRegistrantLoader registrantLoader,
            IRaidSplitConfiguration config,
            ISplitDataStore splitDataStore, 
            IGuildMemberConverter converter)
        {
            this.client = client;
            this.communicator = communicator;
            this.raidSplitter = raidSplitter;
            this.messageBuilder = messageBuilder;
            this.registrantLoader = registrantLoader;
            this.config = config;
            this.splitDataStore = splitDataStore;
            this.converter = converter;
        }

        public async Task RemoveRegistrant(RaidEvent raidEvent, CharacterType characterType, ulong userID)
        {
            var guild = client.GetGuild(raidEvent.GuildID);
            var user = guild?.GetUser(userID);
            if (user == null || user.IsBot)
                return;

            if (raidEvent.Splits.Any())
            {
                //This means the splits are final or the admin is working on building the splits for the event so we need to update the data.
                await RemoveUser(raidEvent, userID, characterType);
            }
            await UpdateAteendeeMessage(raidEvent);
        }

        public async Task RemoveUser(RaidEvent raidEvent, ulong userID, CharacterType characterType)
        {
            var result = raidSplitter.RemoveFromSplit(raidEvent.Splits, userID, characterType);

            if (!result.HasError)
                await updateSplitAnnouncement(raidEvent, result.Split);
            else
                await communicator.SendTell(userID, result.Message);
        }

        public async Task AddRegistrant(RaidEvent raidEvent, CharacterType characterType, GuildMember member, ulong userID)
        {
            var content = $"Thank you for registering to attend {raidEvent.EventName} on {member.CharacterName} as a {characterType}.";
            await communicator.SendMessageByChannelName(raidEvent.GuildID, config.Settings.SpamChannel, content);

            if (!raidEvent.Splits.Any())
                return; //Haven't started split making yet, don't need to do anything else.

            var attendee = converter.ConvertToAttendee(member, characterType, userID);
            var addResult = raidSplitter.AddToSplit(raidEvent, raidEvent.Splits, attendee);

            if (!addResult.HasError)
                await updateSplitAnnouncement(raidEvent, addResult.Split);
            else
                await communicator.SendTell(userID, addResult.Message);

            var mention = MentionUtils.MentionUser(userID);
            await communicator.SendMessageByChannelName(raidEvent.GuildID, config.Settings.SpamChannel, $"{mention} {addResult.Message}");

        }

        public async Task IsNowLate(RaidEvent raidEvent, ulong userID)
        {
            if (!raidEvent.Splits.Any())
                return; //Haven't started splits we don't care if they are late yet.

            List<SplitAttendee> lateCharacters = new List<SplitAttendee>();
            foreach (var split in raidEvent.Splits)
            {
                lateCharacters.AddRange(split.Value.Attendees.Values.Where(x => x.UserID == userID));
            }

            foreach (var lateCharacter in lateCharacters)
            {
                var result = raidSplitter.RemoveFromSplit(raidEvent.Splits, lateCharacter);
                if (!result.HasError)
                {
                    await updateSplitAnnouncement(raidEvent, result.Split);
                    result.Split.Actions.Add((DateTime.Now, $"Removed {result.Attendee.CharacterName}."));

                }
                raidEvent.LateMembers[lateCharacter.CharacterName] = lateCharacter;
            }
            splitDataStore.SaveChanges();

            if (lateCharacters.Any())
                await updateLateAnnouncement(raidEvent);
        }

        public async Task NoLongerLate(RaidEvent raidEvent, ulong userID)
        {
            if (!raidEvent.Splits.Any())
                return; //Splits haven't started, we don't care if they aren't going to be late...

            var lateCharacters = raidEvent.LateMembers.Values.Where(x => x.UserID == userID).ToList();
            foreach (var lateCharacter in lateCharacters)
            {
                var addResult = raidSplitter.AddToSplit(raidEvent, raidEvent.Splits, lateCharacter);
                addResult.Split.Actions.Add((DateTime.Now, $"No Loger Late - Added {addResult.Attendee.CharacterName}"));
                raidEvent.LateMembers.Remove(lateCharacter.CharacterName);
                await updateSplitAnnouncement(raidEvent, addResult.Split);
            }
            splitDataStore.SaveChanges();


            if (lateCharacters.Any())
                await updateLateAnnouncement(raidEvent);
        }

        public async Task WhereAmI(RaidEvent raidEvent, ulong userID)
        {
            var mention = MentionUtils.MentionUser(userID);
            if (!raidEvent.FinalizedDT.HasValue)
            {
                await communicator.SendMessageByChannelName(raidEvent.GuildID, config.Settings.SpamChannel, $"{mention} The Splits aren't final, please wait.");
                return;
            }

            foreach (var split in raidEvent.Splits.Values)
            {
                var characters = split.Attendees.Values.Where(x => x.UserID == userID).Select(x => x.CharacterName);
                if (characters.Any())
                {
                    var characterNames = string.Join(" , ", characters);
                    var message = $"{mention} : {characterNames} are in Split{split.SplitNumber} send an x to {split.Inviter}";
                    await communicator.SendMessageByChannelName(raidEvent.GuildID, config.Settings.SpamChannel, message);
                    return;
                }
            }

            await communicator.SendMessageByChannelName(raidEvent.GuildID, config.Settings.SpamChannel, $"{mention} You are not in a split.");
        }

        public async Task UpdateAteendeeMessage(RaidEvent raidEvent)
        {
            if (raidEvent.Messages.ContainsKey(MessageContexts.AttendeeMessage))
            {
                var message = raidEvent.Messages[MessageContexts.AttendeeMessage];
                var registrants = await registrantLoader.GetRegistrants(raidEvent);
                var content = messageBuilder.BuildAttendeeMessage(raidEvent, registrants);
                await communicator.UpdateMessage(raidEvent.GuildID, message.ChannelID, message.MessageID, content);
            }
        }

        private async Task updateSplitAnnouncement(RaidEvent raidEvent, Split split)
        {
            MessageDetail message = null;
            if (raidEvent.Messages.ContainsKey(MessageContexts.Announcement))
            {
                message = raidEvent.Messages[MessageContexts.Announcement];
            }

            if (raidEvent.Messages.ContainsKey(MessageContexts.FinalAnnouncement))
            {
                message = raidEvent.Messages[MessageContexts.FinalAnnouncement];
            }

            if (message != null)
            {
                var content = messageBuilder.BuildSplitAnnouncement(raidEvent, split);
                await communicator.UpdateMessage(raidEvent.GuildID, message.ChannelID, message.MessageID, content);
            }
        }

        private async Task updateLateAnnouncement(RaidEvent raidEvent)
        {
            MessageDetail lateMessage = null;
            if (raidEvent.Messages.ContainsKey(MessageContexts.LatePreview))
            {
                lateMessage = raidEvent.Messages[MessageContexts.LatePreview];
            }

            if (raidEvent.Messages.ContainsKey(MessageContexts.LateAnnouncement))
            {
                lateMessage = raidEvent.Messages[MessageContexts.LateAnnouncement];
            }

            if (lateMessage != null)
            {
                var lateContent = messageBuilder.BuildLateMessage(raidEvent);
                await communicator.UpdateMessage(raidEvent.GuildID, lateMessage.ChannelID, lateMessage.MessageID, lateContent);

            }
        }


    }
}
