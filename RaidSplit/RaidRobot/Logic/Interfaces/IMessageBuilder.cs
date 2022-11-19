using RaidRobot.Data.Entities;
using RaidRobot.Models;

namespace RaidRobot.Logic
{
    public interface IMessageBuilder
    {
        string BuildAttendeeMessage(RaidEvent raidEvent, RegistrantResponse registrants);
        string BuildRegistrationMessage(RaidEvent raidEvent);
        string BuildSplitAnnouncement(RaidEvent raidEvent, Split split);
        string BuildLateMessage(RaidEvent raidEvent);
    }
}