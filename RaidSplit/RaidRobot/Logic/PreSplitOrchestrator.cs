using RaidRobot.Data.Entities;
using RaidRobot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public class PreSplitOrchestrator : IPreSplitOrchestrator
    {
        private readonly ISplitDataStore splitDataStore;

        public PreSplitOrchestrator(
            ISplitDataStore splitDataStore)
        {
            this.splitDataStore = splitDataStore;
        }

        public string CreatePreSplit(string name, string leader = null, string looter = null, string inviter = null)
        {
            var matchingPreSplit = splitDataStore.PreSplits.Values.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (matchingPreSplit != null)
                return $"PreSplit {name} already Exists, use !rr RemovePreSplit name to delete it.";

            var presplit = new PreSplit()
            {
                Name = name,
            };

            if (!string.IsNullOrWhiteSpace(leader))
            {
                var member = findMember(leader);
                if (member == null)
                    return $"Can't find {leader} in the roster.";

                presplit.LeaderName = member.CharacterName;
                if (!presplit.Characters.Contains(member.CharacterName))
                    presplit.Characters.Add(member.CharacterName);

            }

            if (!string.IsNullOrWhiteSpace(looter))
            {
                var member = findMember(looter);
                if (member == null)
                    return $"Can't find {looter} in the roster.";

                presplit.LooterName = member.CharacterName;
                if (!presplit.Characters.Contains(member.CharacterName))
                    presplit.Characters.Add(member.CharacterName);

            }

            if (!string.IsNullOrWhiteSpace(inviter))
            {
                var member = findMember(inviter);
                if (member == null)
                    return $"Can't find {inviter} in the roster.";

                presplit.InviterName = member.CharacterName;
                if (!presplit.Characters.Contains(member.CharacterName))
                    presplit.Characters.Add(member.CharacterName);
            }

            splitDataStore.PreSplits.TryAdd(name, presplit);
            splitDataStore.SaveChanges();
            return $"Created PreSplit {name}.";
        }

        public string SetRole(string preSplitName, string characterName, RaidResponsibilities responsibility)
        {
            var preSplit = splitDataStore.PreSplits.Values.FirstOrDefault(x => string.Equals(x.Name, preSplitName, StringComparison.OrdinalIgnoreCase));
            if (preSplit == null)
                return $"Can't find PreSplit {preSplitName}";

            var member = findMember(characterName);
            if (member == null)
                return $"Can't find {characterName} in the roster.";

            switch (responsibility)
            {
                case RaidResponsibilities.Leader:
                    preSplit.LeaderName = member.CharacterName;
                    break;
                case RaidResponsibilities.Looter:
                    preSplit.LooterName = member.CharacterName;
                    break;
                case RaidResponsibilities.Inviter:
                    preSplit.InviterName = member.CharacterName;
                    break;
                default:
                    throw new NotImplementedException();
                    break;
            }

            if (!preSplit.Characters.Contains(member.CharacterName))
                preSplit.Characters.Add(member.CharacterName);

            splitDataStore.SaveChanges();
            return $"Updated PreSplit: {preSplitName}, {member.CharacterName} is now the {responsibility}.";
        }

        public string RemovePreSplit(string preSplitName)
        {
            var preSplit = splitDataStore.PreSplits.Values.FirstOrDefault(x => string.Equals(x.Name, preSplitName, StringComparison.OrdinalIgnoreCase));
            if (preSplit == null)
                return $"Can't find PreSplit {preSplitName}";

            if (splitDataStore.PreSplits.TryRemove(preSplit.Name, out var removed))
            {
                splitDataStore.SaveChanges();
                return ($"PreSplit {preSplitName} has been removed.");
            }
            return $"Unable to remove PreSplit {preSplitName}.";
        }

        public string AddCharacters(string preSplitName, string characterNames)
        {
            var preSplit = splitDataStore.PreSplits.Values.FirstOrDefault(x => string.Equals(x.Name, preSplitName, StringComparison.OrdinalIgnoreCase));
            if (preSplit == null)
                return $"Can't find PreSplit {preSplitName}";


            var characters = characterNames.Split(' ');
            foreach (var character in characters)
            {
                var member = findMember(character);
                if (member == null)
                    continue;

                if (!preSplit.Characters.Contains(member.CharacterName))
                    preSplit.Characters.Add(member.CharacterName);
            }

            splitDataStore.SaveChanges();
            return preSplit.ToString();
        }

        public string RemoveCharacters(string preSplitName, string characterNames)
        {
            var preSplit = splitDataStore.PreSplits.Values.FirstOrDefault(x => string.Equals(x.Name, preSplitName, StringComparison.OrdinalIgnoreCase));
            if (preSplit == null)
                return $"Can't find PreSplit {preSplitName}";


            var characters = characterNames.Split(' ');
            foreach (var character in characters)
            {
                var member = findMember(character);
                if (member == null)
                    continue;

                if (preSplit.Characters.Contains(member.CharacterName))
                    preSplit.Characters.Remove(member.CharacterName);
            }

            splitDataStore.SaveChanges();

            return preSplit.ToString();
        }

        public string ViewPreSplits()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var preSplits in splitDataStore.PreSplits)
            {
                sb.AppendLine(preSplits.Value.ToString());
            }
            return sb.ToString();
        }

        private GuildMember findMember(string characterName)
        {
            return splitDataStore.Roster.Values.FirstOrDefault(x => string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
