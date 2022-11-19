﻿using Discord;
using Discord.WebSocket;
using RaidRobot.Data;
using RaidRobot.Data.Entities;
using RaidRobot.Infrastructure;
using RaidRobot.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RaidRobot.Messaging
{
    public class ReactionMonitor : IReactionMonitor
    {
        private readonly DiscordSocketClient client;
        private readonly IRaidSplitConfiguration config;
        private readonly ISplitDataStore splitDataStore;
        private readonly IEventOrchestrator eventOrchestrator;
        private readonly IRosterOrchestrator rosterOrchestrator;

        private Emoji helpEmoji = new Emoji(Reactions.HelpCode);
        private Emoji lateEmoji = new Emoji(Reactions.LateCode);
        private Emoji noShowEmoji = new Emoji(Reactions.NoShowCode);


        public ReactionMonitor(
            DiscordSocketClient client,
            IRaidSplitConfiguration config,
            ISplitDataStore splitDataStore,
            IEventOrchestrator eventOrchestrator,
            IRosterOrchestrator rosterOrchestrator)
        {
            this.client = client;
            this.config = config;
            this.splitDataStore = splitDataStore;
            this.eventOrchestrator = eventOrchestrator;
            this.rosterOrchestrator = rosterOrchestrator;
        }

        public void Initialize()
        {
            client.ReactionAdded += (message, channel, reaction) => { return reactionAdded(message, channel, reaction); };
            client.ReactionRemoved += (message, channel, reaction) => { return reactionRemoved(message, channel, reaction); };
        }

        private async Task reactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            try
            {
                var activeRaid = splitDataStore.Events.Values.FirstOrDefault(x => x.ExpirationDT >= DateTime.Now &&
                    x.Messages.Any(y => y.Key == MessageContexts.RegistrationMessage && y.Value.MessageID == message.Id));

                if (activeRaid == null)
                    return;

                if (isRegistrantEmojiType(activeRaid, reaction, out var characterType))
                {
                    //Don't really care how long this takes, and I don't want to block the handler so not awaiting...
                    var task = eventOrchestrator.RemoveRegistrant(activeRaid, characterType, reaction.UserId);
                }
                else if (reaction.Emote.Name == lateEmoji.Name)
                {
                    //Don't really care how long this takes, and I don't want to block the handler so not awaiting...
                    var task = eventOrchestrator.NoLongerLate(activeRaid, reaction.UserId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Handling Reaction Removed: {ex.Message}");
            }
        }

        private async Task reactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            try
            {
                var activeRaid = splitDataStore.Events.Values.FirstOrDefault(x => x.ExpirationDT >= DateTime.Now &&
                    x.Messages.Any(y => y.Key == MessageContexts.RegistrationMessage && y.Value.MessageID == message.Id));

                if (activeRaid == null)
                    return;


                if (reaction.Emote.Name == helpEmoji.Name)
                {
                    //Not awaiting on purpose...
                    var task = eventOrchestrator.WhereAmI(activeRaid, reaction.UserId);
                    return;
                }

                if (reaction.Emote.Name == lateEmoji.Name)
                {
                    //Not awaiting on purpose...
                    var task = eventOrchestrator.IsNowLate(activeRaid, reaction.UserId);
                    return;
                }

                if (reaction.Emote.Name == noShowEmoji.Name) //No work here, but we also don't want to remove it.
                    return;

                if (isRegistrantEmojiType(activeRaid, reaction, out var characterType))
                {
                    //Not awaiting on purpose..
                    var task = registrantAdded(activeRaid, characterType, activeRaid.GuildID, reaction.UserId, reaction.Channel.Id,  reaction.MessageId, reaction);
                    return;
                }


                //Someone is being goofy and adding reactions we don't care about, get rid of them.
                var channelMessage = await client.GetGuild(activeRaid.GuildID)?.GetTextChannel(channel.Id)?.GetMessageAsync(message.Id);
                await channelMessage.RemoveAllReactionsForEmoteAsync(reaction.Emote);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Handling Reaction Added: {ex.Message}");

            }
        }

        private bool isRegistrantEmojiType(RaidEvent raidEvent, SocketReaction reaction, out CharacterType characterType)
        {
            var retVal = false;
            characterType = null;

            foreach (var cType in raidEvent.RaidType.CharacterTypes)
            {
                var emoji = new Emoji(cType.EmojiCode);
                if (reaction.Emote.Name == emoji.Name)
                {
                    characterType = cType;
                    return true;

                }
            }
            return retVal;
        }

        private async Task registrantAdded(RaidEvent raidEvent, CharacterType characterType, ulong guildID, ulong userID, ulong channelID, ulong messageID, SocketReaction reaction)
        {
            var guild = client.GetGuild(guildID);
            var user = guild.GetUser(userID);

            if (user != null && user.IsBot)
                return;

            var guildMember = await rosterOrchestrator.ValidateUser(raidEvent, reaction.UserId, characterType);
            if (guildMember != null)
            {
                await eventOrchestrator.AddRegistrant(raidEvent, characterType, guildMember, userID);
            }
            else
            {
                //Can't figure out who they are, get rid of em...
                var channel = guild.GetTextChannel(channelID);
                var message = await channel.GetMessageAsync(messageID);

                if(message != null)
                    await message.RemoveReactionAsync(reaction.Emote, userID);
            }
            await eventOrchestrator.UpdateAteendeeMessage(raidEvent);
        }
    }
}