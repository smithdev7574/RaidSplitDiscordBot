using RaidRobot.Data.Entities;
using RaidRobot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public class SplitOrchestrator : ISplitOrchestrator
    {
        public SplitOrchestrator()
        {

        }

        public (SplitAttendee member, string Message) RemoveFromSplit(Split split, string characterName)
        {
            StringBuilder sb = new StringBuilder();

            if (!split.Attendees.ContainsKey(characterName))
                return (null, $"{characterName} is not in Split {split.SplitNumber}.");

            if (string.Equals(split.Leader?.CharacterName, characterName, StringComparison.InvariantCulture))
            {
                sb.AppendLine($"{characterName} is the Leader of Split {split.SplitNumber}. Finding an Alternate.");
                var alternate = split.Attendees.Values.FirstOrDefault(x => x.CanBeLeader && !string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase));
                if (alternate == null)
                    alternate = split.Attendees.Values.FirstOrDefault(x => !string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase));

                sb.AppendLine($"{alternate.CharacterName} is the new Leader of Split {split.SplitNumber}.");
                split.Leader = alternate;
            }

            if (string.Equals(split.MasterLooter?.CharacterName, characterName, StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine($"{characterName} is the Master Looter of Split {split.SplitNumber}. Finding an Alternate.");
                var alternate = split.Attendees.Values.FirstOrDefault(x => x.CanBeMasterLooter && !string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase));
                if (alternate == null)
                    alternate = split.Attendees.Values.FirstOrDefault(x => !string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase));

                sb.AppendLine($"{alternate.CharacterName} is the new Master Looter of Split {split.SplitNumber}.");
                split.MasterLooter = alternate;
            }

            if (string.Equals(split.Inviter?.CharacterName, characterName, StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine($"{characterName} is the Inviter of Split {split.SplitNumber}. Finding an Alternate.");
                var alternate = split.Attendees.Values.FirstOrDefault(x => x.CanBeInviter && !string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase));
                if (alternate == null)
                    alternate = split.Attendees.Values.FirstOrDefault(x => !string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase));

                sb.AppendLine($"{alternate.CharacterName} is the new Inviter of Split {split.SplitNumber}.");
                split.Inviter = alternate;
            }

            var member = split.Attendees[characterName];
            split.Attendees.Remove(characterName);
            member.SplitNumber = null;
            sb.AppendLine($"Removed {characterName} from Split {split.SplitNumber}.");

            return (member, sb.ToString());
        }
    }
}
