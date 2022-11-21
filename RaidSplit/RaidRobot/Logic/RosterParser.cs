using RaidRobot.Data;
using RaidRobot.Data.Entities;
using RaidRobot.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public class RosterParser : IRosterParser
    {
        private readonly ITextCommunicator textCommunicator;
        private readonly ISplitDataStore splitDataStore;

        public RosterParser(
            ITextCommunicator textCommunicator,
            ISplitDataStore splitDataStore)
        {
            this.textCommunicator = textCommunicator;
            this.splitDataStore = splitDataStore;
        }

        public async Task ParseRoster(string attachmentUrl, ulong guildID, ulong channelID)
        {
            try
            {
                var roster = getRoster(attachmentUrl);
                var members = parseMembers(roster);
                var result = splitDataStore.UpdateRoster(members);
                await textCommunicator.SendMessage(guildID, channelID, result);
            }
            catch (Exception ex)
            {
                await textCommunicator.SendMessage(guildID, channelID, "There was an error parsing your file.");
            }
        }

        private string getRoster(string attachmentUrl)
        {
            var client = new WebClient();
            var buffer = client.DownloadData(attachmentUrl);
            var download = Encoding.UTF8.GetString(buffer);
            return download;
        }

        private List<GuildMember> parseMembers(string roster)
        {
            var members = new List<GuildMember>();
            var lines = roster.Split(Environment.NewLine);
            foreach (var line in lines)
            {
                var member = parseMember(line);
                if (member != null)
                    members.Add(member);
            }
            return members;
        }

        private GuildMember parseMember(string line)
        {
            var fields = line.Split('\t');
            if (fields.Length < 13)
                return null;

            if (!int.TryParse(fields[1], out var level))
            {
                var test = fields[1];
            }

            var member = new GuildMember()
            {
                CharacterName = fields[0],
                Level = level,
                ClassName = fields[2],
                Rank = fields[3],
                Comment = fields[13],
            };
            return member;
        }
    }
}
