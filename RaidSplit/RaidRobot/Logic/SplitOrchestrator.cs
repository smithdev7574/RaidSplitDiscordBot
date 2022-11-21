using RaidRobot.Data.Entities;
using RaidRobot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaidRobot.Infrastructure;

namespace RaidRobot.Logic
{
    public class SplitOrchestrator : ISplitOrchestrator
    {
        private readonly ISplitDataStore splitDataStore;
        private readonly IGuildMemberConverter converter;

        public SplitOrchestrator(
            ISplitDataStore splitDataStore,
            IGuildMemberConverter converter)
        {
            this.splitDataStore = splitDataStore;
            this.converter = converter;
        }

        public (SplitAttendee member, string Message) RemoveFromSplit(Split split, string characterName)
        {
            StringBuilder sb = new StringBuilder();

            var attendee = split.Attendees.Values.FirstOrDefault(x => string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase));
            if (attendee == null)
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

            split.Attendees.Remove(attendee.CharacterName);
            split.Actions.Add((DateTime.Now, $"Removed {attendee.CharacterName}"));
            attendee.SplitNumber = null;
            sb.AppendLine($"Removed {characterName} from Split {split.SplitNumber}.");

            return (attendee, sb.ToString());
        }

        public string UpdateRole(RaidEvent raidEvent, Dictionary<int, Split> splits, Split split, string characterName, RaidResponsibilities resposibility)
        {
            StringBuilder sb = new StringBuilder();
            var member = splitDataStore.Roster.Values.FirstOrDefault(x => string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase));
            if (member == null)
                return $"Sorry, {characterName} is not in the Roster.";

            var characterType = raidEvent.RaidType.CharacterTypes.FirstOrDefault(x => string.Equals(x.Name, member.CharacterType));
            if (characterType == null)
                return $"Sorry, {characterName}'s is a {member.CharacterType} which is not allowed in Raid Type {raidEvent.RaidType.Name}.";

            //Check to See if they are already in a split
            var existingSplit = FindSplitByCharacter(splits, characterName);

            //Add them to the split if they aren't in one
            if (existingSplit == null)
            {
                var attendee = converter.ConvertToAttendee(member, characterType, member.UserId ?? 0);
                split.Attendees[attendee.CharacterName] = attendee;
                sb.AppendLine($"Added {characterName} to Split {split.SplitNumber}");
            }

            //If they are in a different split
            if (existingSplit != null && existingSplit.SplitNumber != split.SplitNumber)
            {
                var result = RemoveFromSplit(existingSplit, characterName);
                sb.AppendLine(result.Message);
                split.Attendees[result.member.CharacterName] = result.member;
                sb.AppendLine($"Added {characterName} to Split {split.SplitNumber}");
            }

            //Set them to the new role in the split
            var message = SetRole(split, characterName, resposibility);
            sb.AppendLine(message);

            //If they were in a different split See if you can Replace Them with someone else of the same class
            if (existingSplit != null && existingSplit.SplitNumber != split.SplitNumber)
            {
                sb.AppendLine($"Looking for replacement to add to Split {existingSplit.SplitNumber}.");
                var replacement = FindReplacementFor(split, characterName);
                if (replacement.Attendee != null)
                {
                    var removed = RemoveFromSplit(split, replacement.Attendee.CharacterName);
                    sb.AppendLine(removed.Message);
                    existingSplit.Attendees[removed.member.CharacterName] = removed.member;
                    sb.AppendLine($"Added {replacement} to Split {existingSplit.SplitNumber}");
                }
                else
                {
                    sb.AppendLine($"Couldn't find a suitable replacement for {replacement.Message}");
                }
            }

            splitDataStore.SaveChanges();
            return sb.ToString();
        }

        public Split FindSplitByCharacter(Dictionary<int, Split> splits, string characterName)
        {
            return splits.Values.FirstOrDefault(x => x.Attendees.Values
                .Any(y => string.Equals(y.CharacterName, characterName, StringComparison.OrdinalIgnoreCase)));
        }

        public string SetRole(Split split, string characterName, RaidResponsibilities resposibility)
        {
            var attendee = split.Attendees.Values.FirstOrDefault(x => string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase));
            if (attendee == null)
                return $"{characterName} is not in Split {split.SplitNumber}.";

            switch (resposibility)
            {
                case RaidResponsibilities.Leader:
                    split.Leader = attendee;
                    return $"{characterName} is now the Leader of Split {split.SplitNumber}.";
                case RaidResponsibilities.Looter:
                    split.MasterLooter = attendee;
                    return $"{characterName} is now the Master Looter of Split {split.SplitNumber}.";
                case RaidResponsibilities.Inviter:
                    split.Inviter = attendee;
                    return $"{characterName} is now the Inviter of Split {split.SplitNumber}.";
            }

            return $"I don't understand {resposibility}";
        }

        public (SplitAttendee Attendee, string Message) FindReplacementFor(Split split, string characterName)
        {
            var attendee = split.Attendees.Values.FirstOrDefault(x => string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase));
            if (attendee == null)
                return (null, $"{characterName} is not in Split {split.SplitNumber}.");


            var alternate = split.Attendees.Values.FirstOrDefault(x => !string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase)
                && !x.IsAnchor
                && !x.IsBox
                && !x.IsBoxing
                && string.Equals(x.ClassName, attendee.ClassName, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(x.CharacterName, split.Leader.CharacterName, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(x.CharacterName, split.Leader.CharacterName, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(x.CharacterName, split.MasterLooter.CharacterName, StringComparison.OrdinalIgnoreCase));

            if (alternate == null)
                alternate = split.Attendees.Values.FirstOrDefault(x => !string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase)
                && !x.IsAnchor
                && !x.IsBox
                && !x.IsBoxing
                && string.Equals(x.ClassName, attendee.ClassName, StringComparison.OrdinalIgnoreCase));

            if (alternate == null)
                alternate = split.Attendees.Values.FirstOrDefault(x => !string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase)
                && !x.IsBox
                && !x.IsBoxing);

            return (alternate, string.Empty);
        }

        public string MoveTo(RaidEvent raidEvent, Dictionary<int, Split> splits, string characterName, int splitNumber, bool skipBoxes = false)
        {
            StringBuilder sb = new StringBuilder();

            var split = splits.Values.FirstOrDefault(x => x.SplitNumber == splitNumber);
            if (split == null)
                return $"Could not find a Split {splitNumber}";

            SplitAttendee splitMember = null;
            var existingSplit = FindSplitByCharacter(splits, characterName);
            if (existingSplit == null)
            {
                sb.AppendLine($"{characterName} is not in a split.");
                var member = splitDataStore.Roster.Values.FirstOrDefault(x => string.Equals(x.CharacterName, characterName, StringComparison.OrdinalIgnoreCase));
                if (member == null)
                    return $"Sorry, {characterName} is not in the Roster.";

                var characterType = raidEvent.RaidType.CharacterTypes.FirstOrDefault(x => string.Equals(x.Name, member.CharacterType));
                if (characterType == null)
                    return $"Sorry, {characterName}'s is a {member.CharacterType} which is not allowed in Raid Type {raidEvent.RaidType.Name}.";

                splitMember = converter.ConvertToAttendee(member, characterType, member.UserId ?? 0);
            }
            else
            {
                var result = RemoveFromSplit(existingSplit, characterName);
                sb.Append(result.Message);
                splitMember = result.member;
            }

            var addMessage = addToSplit(split, splitMember);
            sb.AppendLine(addMessage);

            if (!skipBoxes)
            {
                if (!splitMember.IsBox)
                {
                    var isBoxing = splits.SelectMany(x => x.Value.Attendees.Values).Any(x => x.UserID == splitMember.UserID && x.IsBox);

                    if (isBoxing)
                    {
                        var message = moveBox(raidEvent, splitMember.CharacterName, splits, splitMember.UserID, split.SplitNumber);
                        sb.AppendLine(message);
                    }
                }
            }

            splitDataStore.SaveChanges();
            return sb.ToString();
        }

        public string Swap(RaidEvent raidEvent, Dictionary<int, Split> splits, string characterName1, string characterName2)
        {
            StringBuilder sb = new StringBuilder();
            var split1 = FindSplitByCharacter(splits, characterName1);
            if (split1 == null)
                return $"{characterName1} is not in a split.";
            var split2 = FindSplitByCharacter(splits, characterName2);
            if (split2 == null)
                return $"{characterName2} is not in a split.";

            if (split1.SplitNumber == split2.SplitNumber)
                return $"{characterName1} and {characterName2} are already in the same split.";

            var result1 = RemoveFromSplit(split1, characterName1);
            sb.Append(result1.Message);
            var result2 = RemoveFromSplit(split2, characterName2);
            sb.Append(result2.Message);

            split2.Attendees[result1.member.CharacterName] = result1.member;
            sb.AppendLine($"Added {result1.member.CharacterName} to Split {split2.SplitNumber}");
            split1.Attendees[result2.member.CharacterName] = result2.member;
            sb.AppendLine($"Added {result2.member.CharacterName} to Split {split1.SplitNumber}");

            if (result1.member.IsBoxing)
            {
                var move = moveBox(raidEvent, result1.member.CharacterName, splits, result1.member.UserID, split2.SplitNumber);
                sb.AppendLine(move);
            }

            if (result2.member.IsBoxing)
            {
                var move = moveBox(raidEvent, result2.member.CharacterName, splits, result2.member.UserID, split1.SplitNumber);
                sb.AppendLine(move);
            }

            splitDataStore.SaveChanges();
            return sb.ToString();
        }

        private string addToSplit(Split split, SplitAttendee attendee)
        {
            split.Attendees[attendee.CharacterName] = attendee; ;
            attendee.SplitNumber = split.SplitNumber;
            split.Actions.Add((DateTime.Now, $"Added {attendee.CharacterName}"));
            return $"Added {attendee.CharacterName} to Split {attendee.SplitNumber}.";
        }

        private string moveBox(RaidEvent raidEvent, string mainCharacterName, Dictionary<int, Split> splits, ulong userID, int newSplit)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var split in splits.Values.Where(x => x.SplitNumber != newSplit).ToList())
            {
                var boxes = split.Attendees.Values.Where(x => x.UserID == userID
                    && !string.Equals(x.CharacterName, mainCharacterName, StringComparison.OrdinalIgnoreCase)).ToList();

                foreach (var box in boxes)
                {
                    var result = MoveTo(raidEvent, splits, box.CharacterName, newSplit, true);
                    sb.Append(result);
                }
            }
            return sb.ToString();
        }




    }
}
