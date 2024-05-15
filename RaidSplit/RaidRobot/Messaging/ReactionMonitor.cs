using Discord;
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
        private readonly ITextCommunicator textCommunicator;

        private Emoji helpEmoji = new Emoji(Reactions.HelpCode);
        private Emoji lateEmoji = new Emoji(Reactions.LateCode);
        private Emoji noShowEmoji = new Emoji(Reactions.NoShowCode);


        public ReactionMonitor(
            DiscordSocketClient client,
            IRaidSplitConfiguration config,
            ISplitDataStore splitDataStore,
            IEventOrchestrator eventOrchestrator,
            IRosterOrchestrator rosterOrchestrator,
            ITextCommunicator textCommunicator)
        {
            this.client = client;
            this.config = config;
            this.splitDataStore = splitDataStore;
            this.eventOrchestrator = eventOrchestrator;
            this.rosterOrchestrator = rosterOrchestrator;
            this.textCommunicator = textCommunicator;
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
            try
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

                    if (message != null)
                    {
                        try
                        {
                            await message.RemoveReactionAsync(reaction.Emote, userID);
                        }
                        catch (Exception ex) 
                        {
                            Console.WriteLine($"Unable to remove reaction from registration message: {ex.Message}");
                            await textCommunicator.SendMessageByChannelName(raidEvent.GuildID, config.Settings.SpamChannel, $"Unable to remove reaction from registration message: {ex.Message}");
                        }
                        var mention = MentionUtils.MentionUser(userID);
                        var characters = splitDataStore.Roster.Values.Where(x => x.UserId == userID);
                        string charactersMessage = "none";
                        if (characters.Any())
                            charactersMessage = string.Join(", ", characters.Select(x => $"{x.CharacterName} - {x.CharacterType}"));
                        var errorMessage = $"Removing you from {raidEvent.EventName} because there is something wrong with your {characterType.Name} character. Most likely I don't know your character name or your class. Type !rr Help in the bot spam channel to learn how to set your class.{Environment.NewLine}" +
                            $"Characters mapped to you: {charactersMessage}";
                        await textCommunicator.SendTell(reaction.UserId, errorMessage);
                    }
                }
                await eventOrchestrator.UpdateAteendeeMessage(raidEvent);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error registrantAdded: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                await textCommunicator.SendMessageByChannelName(raidEvent.GuildID, config.Settings.SpamChannel, $"Error adding registrant: {ex.Message}");
            }
        }
    }
}
