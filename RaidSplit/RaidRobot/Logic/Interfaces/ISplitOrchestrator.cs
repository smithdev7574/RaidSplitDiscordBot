using RaidRobot.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public interface ISplitOrchestrator
    {
        (SplitAttendee member, string Message) RemoveFromSplit(Split split, string characterName);
        string UpdateRole(RaidEvent raidEvent, Dictionary<int, Split> splits, Split split, string characterName, RaidResponsibilities resposibility);
        Split FindSplitByCharacter(Dictionary<int, Split> splits, string characterName);
        string SetRole(Split split, string characterName, RaidResponsibilities resposibility);
        (SplitAttendee Attendee, string Message) FindReplacementFor(Split split, string characterName);
        string MoveTo(RaidEvent raidEvent, Dictionary<int, Split> splits, string characterName, int splitNumber, bool skipBoxes = false);
        string Swap(RaidEvent raidEvent, Dictionary<int, Split> splits, string characterName1, string characterName2);

    }
}