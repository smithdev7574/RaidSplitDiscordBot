using Discord;
using RaidRobot.Data.Entities;
using RaidRobot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using RaidRobot.Infrastructure;
using System.Diagnostics.Metrics;
using RaidRobot.Messaging;
using System.Reflection.Metadata;

namespace RaidRobot.Logic
{
    public class RosterOrchestrator : IRosterOrchestrator
    {
        private readonly DiscordSocketClient client;
        private readonly ISplitDataStore splitDataStore;
        private readonly IRaidSplitConfiguration config;
        private readonly ITextCommunicator textCommunicator;

        public RosterOrchestrator(
            DiscordSocketClient client,
            ISplitDataStore splitDataStore,
            IRaidSplitConfiguration config,
            ITextCommunicator textCommunicator)
        {
            this.client = client;
            this.splitDataStore = splitDataStore;
            this.config = config;
            this.textCommunicator = textCommunicator;
        }

        public GuildMember FindMember(ulong userID, CharacterType characterType)
        {
            var member = splitDataStore.Roster.Values.FirstOrDefault(x => x.UserId == userID && string.Equals(x.CharacterType, characterType.Name, StringComparison.OrdinalIgnoreCase));
            return member;
        }

        public async Task<GuildMember> MapUser(ulong guildID, ulong userID, string characterName, CharacterType characterType)
        {
            var anotherCharacter = FindMember(userID, characterType);
            var matchingCharacter = splitDataStore.Roster.Values.FirstOrDefault(x => string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase));

            if (matchingCharacter == null)
            {
                matchingCharacter = new GuildMember()
                {
                    CharacterName = characterName,
                };
                splitDataStore.Roster.TryAdd(characterName, matchingCharacter);
            }

            matchingCharacter.UserId = userID;
            matchingCharacter.CharacterType = characterType.Name;

            var mention = MentionUtils.MentionUser(userID);

            string deregister = string.Empty;
            if (anotherCharacter != null && anotherCharacter.CharacterName != matchingCharacter.CharacterName)
            {
                anotherCharacter.UserId = null;
                anotherCharacter.CharacterType = null;
                deregister = $" {anotherCharacter.CharacterName} has been removed as {mention} 's {characterType}.";
            }

            splitDataStore.SaveChanges();
            string content = $"{characterName} has been Registered to {mention} as a {characterType}.{deregister} Thank You!";

            await textCommunicator.SendMessageByChannelName(guildID, config.Settings.SpamChannel, content);
            return matchingCharacter;
        }


        public async Task<GuildMember> AutoMapCharacter(RaidEvent raidEvent, ulong userID, CharacterType characterType)
        {
            var Guild = client.GetGuild(raidEvent.GuildID);
            var guildUser = Guild?.GetUser(userID);
            var userName = guildUser?.Nickname ?? guildUser?.Username;

            if (string.IsNullOrEmpty(userName))
                return null; //Can't map anything if we don't know the username...

            var match = splitDataStore.Roster.Values
                .FirstOrDefault(x => !x.UserId.HasValue
                    && string.Equals(x.CharacterName, userName, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                return await MapUser(raidEvent.GuildID, userID, match.CharacterName, characterType);
            }

            //See if their username is the comment of another character in the guild (boxes)...
            match = splitDataStore.Roster.Values
                .FirstOrDefault(x => !x.UserId.HasValue
                    && string.Equals(x.Comment, userName, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                return await MapUser(raidEvent.GuildID, userID, match.CharacterName, characterType);
            }

            return null;
        }

        public async Task<GuildMember> ValidateUser(RaidEvent raidEvent, ulong userID, CharacterType characterType)
        {
            var guild = client.GetGuild(raidEvent.GuildID);
            var user = guild.GetUser(userID);

            var member = FindMember(userID, characterType);
            if (member == null)
            {
                member = await AutoMapCharacter(raidEvent, userID, characterType);
            }

            var mention = MentionUtils.MentionUser(userID);
            if (member == null)
            {
                var content = $"I don't know {mention} 's {characterType.Name}. Please reply to this message with their character name." +
                    $" You must do this before raid time to be included in a split. You should only have to do this once.";
                var messageResult = await textCommunicator.SendMessageByChannelName(raidEvent.GuildID, config.Settings.SpamChannel, content);

                var unknownMessage = new UnknownMessage()
                {
                    GuildID = raidEvent.GuildID,
                    ChannelID = messageResult.Channel.Id,
                    MessageID = messageResult.Id,
                    CharacterType = characterType.Name,
                    MessageType = UnknownMessageTypes.Character,
                    UserID = userID,
                    UserName = user?.Nickname ?? user?.Username,
                };
                splitDataStore.UnknownMessages.TryAdd(unknownMessage.MessageID, unknownMessage);
                splitDataStore.SaveChanges();
                return null;
            }

            if (string.IsNullOrEmpty(member.ClassName))
            {
                var classMessage = $"I don't know {member.CharacterName}'s class. Please reply to this message with {member.CharacterName}'s class." +
                $"Please do this before raid time or the split will ignore you. You should only have to do this once. " +
                $"{Environment.NewLine}**Acceptable Values:** {string.Join(", ", config.Classes.Select(x => x.Name).OrderBy(x => x))}";
                var messageResult = await textCommunicator.SendMessageByChannelName(raidEvent.GuildID, config.Settings.SpamChannel, classMessage);

                var unknownMessage = new UnknownMessage()
                {
                    GuildID = raidEvent.GuildID,
                    ChannelID = messageResult.Channel.Id,
                    MessageID = messageResult.Id,
                    CharacterType = characterType.Name,
                    MessageType = UnknownMessageTypes.Class,
                    CharacterName = member.CharacterName,
                    UserID = userID,
                    UserName = user?.Nickname ?? user?.Username,
                };
                splitDataStore.UnknownMessages.TryAdd(unknownMessage.MessageID, unknownMessage);
                splitDataStore.SaveChanges();

                return null;
            }

            return member;
        }
    }
}
