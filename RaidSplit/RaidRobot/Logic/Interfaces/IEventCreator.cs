using Discord;
using Discord.Commands;
using RaidRobot.Data.Entities;
using System;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public interface IEventCreator
    {
        string BuildAttendeeMessage(RaidEvent raidEvent);
        string BuildRegistrationMessage(RaidEvent raidEvent);
        Task CreateEvent(SocketCommandContext context, IGuildUser user, string eventName, DateTime raidTime, string raidTypeName);
    }
}