using RaidRobot.Data.Entities;

namespace RaidRobot.Logic
{
    public interface ISplitOrchestrator
    {
        (SplitAttendee member, string Message) RemoveFromSplit(Split split, string characterName);
    }
}