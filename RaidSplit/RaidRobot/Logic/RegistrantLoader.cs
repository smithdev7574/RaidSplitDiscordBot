using Discord;
using Discord.WebSocket;
using RaidRobot.Data;
using RaidRobot.Data.Entities;
using RaidRobot.Messaging;
using RaidRobot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public class RegistrantLoader : IRegistrantLoader
    {
        private readonly DiscordSocketClient client;
        private readonly ISplitDataStore splitDataStore;
        private readonly IGuildMemberConverter converter;

        public RegistrantLoader(
            DiscordSocketClient client,
            ISplitDataStore splitDataStore,
            IGuildMemberConverter converter)
        {
            this.client = client;
            this.splitDataStore = splitDataStore;
            this.converter = converter;
        }

        public async Task<RegistrantResponse> GetRegistrants(RaidEvent raidEvent)
        {
            var reactions = await GetReactions(raidEvent);
            var lateUserIDs = await GetLateRegistrants(raidEvent);
            var response = loadMembers(reactions, lateUserIDs);
            return response;
        }

        public async Task<List<(CharacterType CharacterType, IUser User)>> GetReactions(RaidEvent raidEvent)
        {
            if (!raidEvent.Messages.ContainsKey(MessageContexts.RegistrationMessage))
                throw new ArgumentException("Cannot get reactions, the registration message does not exist.");

            var reactors = new List<(CharacterType CharacterType, IUser User)>();
            var guild = client.GetGuild(raidEvent.GuildID);
            if (guild == null)
                return null;

            var channel = guild.GetTextChannel(raidEvent.Messages[MessageContexts.RegistrationMessage].ChannelID);
            if (channel == null)
                return null;

            var message = await channel.GetMessageAsync(raidEvent.Messages[MessageContexts.RegistrationMessage].MessageID);
            if (message == null)
                return null;

            foreach (var characterType in raidEvent.RaidType.CharacterTypes)
            {
                var emoji = new Emoji(characterType.EmojiCode);
                var reactions = await getReactions(characterType.EmojiCode, message);
                reactors.AddRange(reactions.Select(x => (characterType, x)));
            }

            return reactors;
        }

        public async Task<List<ulong>> GetLateRegistrants(RaidEvent raidEvent)
        {
            if (!raidEvent.Messages.ContainsKey(MessageContexts.RegistrationMessage))
                throw new ArgumentException("Cannot get reactions, the registration message does not exist.");

            var guild = client.GetGuild(raidEvent.GuildID);
            if (guild == null)
                return null;

            var channel = guild.GetTextChannel(raidEvent.Messages[MessageContexts.RegistrationMessage].ChannelID);
            if (channel == null)
                return null;

            var message = await channel.GetMessageAsync(raidEvent.Messages[MessageContexts.RegistrationMessage].MessageID);
            if (message == null)
                return null;

            return (await getReactions(Reactions.LateCode, message)).Select(x => x.Id).ToList();
        }

        private RegistrantResponse loadMembers(List<(CharacterType CharacterType, IUser User)> reactions, List<ulong> lateUserIDs)
        {
            var response = new RegistrantResponse();

            foreach (var reaction in reactions)
            {
                var guildMember = splitDataStore.Roster.Values
                    .FirstOrDefault(x => x.UserId == reaction.User.Id
                        && string.Equals(x.CharacterType, reaction.CharacterType.Name, StringComparison.OrdinalIgnoreCase));

                if (guildMember == null)
                {
                    response.UnknownUsers.Add(new UnknownUser()
                    {
                        UserID = reaction.User.Id,
                        UserName = reaction.User.Username,
                        characterType = reaction.CharacterType.Name,
                    });
                    continue;
                }

                var splitMember = converter.ConvertToAttendee(guildMember, reaction.CharacterType, reaction.User.Id);
                if (lateUserIDs.Contains(splitMember.UserID))
                    splitMember.IsLate = true;

                if (string.IsNullOrWhiteSpace(splitMember.ClassName))
                {
                    response.IncompleteUsers.Add(splitMember);
                    continue;
                }

                response.Members[splitMember.CharacterName] = splitMember;
            }
            return response;
        }

        private async Task<List<IUser>> getReactions(string emojiCode, IMessage message)
        {
            var emoji = new Emoji(emojiCode);
            var getter = message.GetReactionUsersAsync(emoji, 1000);
            return (await getter.FlattenAsync()).Where(x => !x.IsBot).ToList();
        }

    }

}
