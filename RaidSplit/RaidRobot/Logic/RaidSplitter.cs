using RaidRobot.Data.Entities;
using RaidRobot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaidRobot.Models;

namespace RaidRobot.Logic
{
    public class RaidSplitter : IRaidSplitter
    {
        private readonly ISplitDataStore splitDataStore;
        private readonly IGuildMemberConverter converter;
        private readonly ISplitOrchestrator splitOrchestrator;

        public RaidSplitter(
            ISplitDataStore splitDataStore,
            IGuildMemberConverter converter,
            ISplitOrchestrator splitOrchestrator)
        {
            this.splitDataStore = splitDataStore;
            this.converter = converter;
            this.splitOrchestrator = splitOrchestrator;
        }

        public AttendeeResponse RemoveFromSplit(Dictionary<int, Split> splits, ulong userId, CharacterType characterType)
        {
            var member = splitDataStore.Roster.Values
                    .FirstOrDefault(x => x.UserId == userId
                        && string.Equals(x.CharacterType, characterType.Name, StringComparison.OrdinalIgnoreCase));

            if (member == null)
            {
                return new AttendeeResponse()
                {
                    HasError = true,
                    Message = $"Sorry I don't know which character is your {characterType}.  I cannot remove you from a split.",
                };
            }

            if (string.IsNullOrEmpty(member.ClassName))
            {
                return new AttendeeResponse()
                {
                    HasError = true,
                    Message = $"Sorry, I don't know what class {member.CharacterName} is. I cannot remove you from a split.",
                };
            }

            var attendee = converter.ConvertToAttendee(member, characterType, userId);
            return RemoveFromSplit(splits, attendee);
        }

        public AttendeeResponse RemoveFromSplit(Dictionary<int, Split> splits, SplitAttendee splitAttendee)
        {
            var split = splits.Values.FirstOrDefault(x => x.Attendees.Values.Any(y => y.CharacterName == splitAttendee.CharacterName));
            if (split == null)
            {
                return new AttendeeResponse()
                {
                    HasError = true,
                    Message = $"You are not in a Split.",
                };
            }

            splitOrchestrator.RemoveFromSplit(split, splitAttendee.CharacterName);
            split.Actions.Add((DateTime.Now, $"Removed {splitAttendee.CharacterName}."));
            splitDataStore.SaveChanges();

            return new AttendeeResponse()
            {
                Message = $"{splitAttendee.CharacterName} removed from Split {split.SplitNumber}.",
                Split = split,
                Attendee = splitAttendee
            };
        }

        public  AttendeeResponse AddToSplit(RaidEvent raidEvent, Dictionary<int, Split> splits, ulong userId, CharacterType characterType, bool ignoreBuddies = false)
        {
            var member = splitDataStore.Roster.Values
                    .FirstOrDefault(x => x.UserId == userId
                        && string.Equals(x.CharacterType, characterType.Name, StringComparison.OrdinalIgnoreCase));

            if (member == null)
            {
                return new AttendeeResponse()
                {
                    HasError = true,
                    Message = $"Sorry I don't know which character is your {characterType}.  I cannot add you to a split.",
                };
            }

            if (string.IsNullOrEmpty(member.ClassName))
            {
                return new AttendeeResponse()
                {
                    HasError = true,
                    Message = $"Sorry, I don't know what class {member.CharacterName} is. I cannot add you to a split.",
                };
            }

            var attendee = converter.ConvertToAttendee(member, characterType, userId);
            return AddToSplit(raidEvent, splits, attendee, ignoreBuddies);
        }

        public  AttendeeResponse AddToSplit(RaidEvent raidEvent, Dictionary<int, Split> splits, SplitAttendee attendee, bool ignoreBuddies = false)
        {
            var alreadyInASplit = raidEvent.Splits.Values.FirstOrDefault(x => x.Attendees.Values.Any(y => string.Equals(y.CharacterName, attendee.CharacterName, StringComparison.OrdinalIgnoreCase)));
            if (alreadyInASplit != null)
            {
                return new AttendeeResponse()
                {
                    HasError = true,
                    Message = $"You are already in Split {alreadyInASplit.SplitNumber} Send an X to {alreadyInASplit.Inviter.CharacterName}.",
                    Split = alreadyInASplit,
                    Attendee = attendee
                };
            }

            var splitResult = findBestSplitForAttendee(raidEvent, splits, attendee, ignoreBuddies);
            var bestSplit = splitResult.Split;

            //Check to see if your are already in a split with another character.
            var existingSplit = splits.Values.FirstOrDefault(x => x.Attendees.Values.Any(y => y.UserID == attendee.UserID));
            if (existingSplit != null)
            {
                attendee.IsBoxing = true;
                bestSplit = existingSplit;
            }

            addSplitAttendee(splits, bestSplit, attendee, Enumerable.Empty<SplitAttendee>(), splitResult.Audit);
            bestSplit.Actions.Add((DateTime.Now, $"Added {attendee.CharacterName}."));
            splitDataStore.SaveChanges();

            return new AttendeeResponse()
            {
                Message = $"{attendee.CharacterName} added to Split {bestSplit.SplitNumber}. Send an X to {bestSplit.Inviter.CharacterName}",
                Split = bestSplit,
                Attendee = attendee
            };
        }

        private (Split Split, string Audit) findBestSplitForAttendee(RaidEvent raidEvent, Dictionary<int, Split> splits, SplitAttendee attendee, bool ignoreBuddies)
        {
            StringBuilder audit = new StringBuilder();
            Split bestSplit = null;

            //Default Logic
            if (attendee.IsAnchor)
            {
                bestSplit = findLowestCountSplit(splits, attendee.ClassName);
                audit.AppendLine($"{generateAuditStart(bestSplit, attendee)} they are an anchor and this split had the lowest **{attendee.ClassName}s**.");
            }
            else
            {
                bestSplit = findLowestCountSplit(splits, attendee.ClassName);
                audit.AppendLine($"{generateAuditStart(bestSplit, attendee)} this split had the lowest **{attendee.ClassName}s**.");
            }

            //Check if we should swap due to buddies
            if (!string.IsNullOrEmpty(attendee.BuddieGroup) && !ignoreBuddies)
            {
                var buddieSplit = splits.Values.FirstOrDefault(x => x.Attendees.Values.Any(x => x.BuddieGroup == attendee.BuddieGroup));
                audit.AppendLine("Checking Splits For Existing Buddies..");
                if (buddieSplit != null && buddieSplit.SplitNumber != bestSplit.SplitNumber)
                {
                    audit.AppendLine($"Moved {attendee.CharacterName} from Split {bestSplit.SplitNumber} to {buddieSplit.SplitNumber} because their buddies are in this split.");
                    bestSplit = buddieSplit;
                }
                else
                {
                    audit.AppendLine("No Buddies Found...");
                }
            }

            //Check if we should swap due to item needs
            var neededItems = raidEvent.ItemNeeds.Where(x => string.Equals(x.CharacterName, attendee.CharacterName, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Item).Distinct().ToList();

            foreach (var neededItem in neededItems)
            {
                audit.AppendLine($"Checking to see if anyone in Split {bestSplit.SplitNumber} needs {neededItem}");

                var charactersWhoNeedTheItem = raidEvent.ItemNeeds.Where(x => x.Item == neededItem).Select(x => x.CharacterName.ToLower()).Distinct().ToList();
                var charactersInSplit = checkForCharacters(bestSplit, charactersWhoNeedTheItem);

                if (charactersInSplit.Any())
                {
                    audit.AppendLine($"{string.Join(",", charactersInSplit)} need {neededItem}");

                    //Look for a split without someone who needs the items, if we can't find one we will just go with what we had.
                    foreach (var split in splits.Values.Where(x => x.SplitNumber != bestSplit.SplitNumber))
                    {
                        audit.AppendLine($"Checking to see if anyone in Split {split.SplitNumber} needs {neededItem}");
                        var matchingCharactersInSplit = checkForCharacters(split, charactersWhoNeedTheItem);

                        if (!matchingCharactersInSplit.Any())
                        {
                            audit.AppendLine($"Moved {attendee.CharacterName} from Split {bestSplit.SplitNumber} to {split.SplitNumber} because they need {neededItem}.");
                            bestSplit = split;
                            break;
                        }
                        else
                        {
                            audit.AppendLine($"{string.Join(",", matchingCharactersInSplit)} need {neededItem}");
                        }
                    }
                }
                else
                {
                    audit.AppendLine($"No one in Split {bestSplit.SplitNumber} needs {neededItem}");
                }
            }

            if (bestSplit == null)
            {
                audit.AppendLine($"No logic found taking the first split...");
                bestSplit = splits.Values.First();
            }

            return (bestSplit, audit.ToString());
        }

        private Split findLowestCountSplit(Dictionary<int, Split> splits, string className)
        {
            var lowestSplit = splits.Values.OrderBy(x => x.ClassWeight(className)).ThenBy(x => x.Attendees.Count()).FirstOrDefault();

            if (lowestSplit == null)
                lowestSplit = splits.Values.First();

            return lowestSplit;
        }

        private string generateAuditStart(Split split, SplitAttendee attendee)
        {
            return $"Adding **{attendee.CharacterName}** to **Split: {split.SplitNumber}** because";
        }
        private List<string> checkForCharacters(Split split, List<string> characters)
        {
            var members = split.Attendees.Values.Select(x => x.CharacterName.ToLower());
            return members.Intersect(characters).ToList();
        }

        private void addSplitAttendee(Dictionary<int, Split> splits, Split split, SplitAttendee attendee, IEnumerable<SplitAttendee> attendees, string reason)
        {
            if (attendee.SplitNumber != null)
                return;

            var splitAudit = string.Join(Environment.NewLine, splits.Select(x => x.Value.ToString()));
            split.Attendees[attendee.CharacterName] = attendee;
            var count = split.Attendees.Count.ToString().PadLeft(4, '0');
            attendee.SplitNumber = split.SplitNumber;
            attendee.SplitReason = $"{count}{Environment.NewLine}{splitAudit}{Environment.NewLine}{reason}";

            if (!attendee.IsBox)
            {
                var box = attendees.FirstOrDefault(x => x.UserID == attendee.UserID && x.IsBox);
                if (box != null)
                {
                    addSplitAttendee(splits, split, box, attendees, $"{generateAuditStart(split, box)} they are **{attendee.CharacterName}'s** box.");
                    attendee.IsBoxing = true;
                }
            }
        }
    }
}
