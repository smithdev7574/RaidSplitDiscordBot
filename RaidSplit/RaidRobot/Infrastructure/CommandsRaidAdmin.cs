using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Infrastructure
{
    public partial class Commands
    {
        [Command("CreateEvent", RunMode = RunMode.Async)]
        public async Task CreateEvent(string eventName, string raidType, [Remainder] string eventDate)
        {
            var permissionResult = permissionChecker.CheckManagerPermissions(Context, "Create Event");
            if (!permissionResult.HasPremission)
            {
                await ReplyAsync(permissionResult.Message);
                return;
            }

            if (!DateTime.TryParse(eventDate, out var eventTime))
            {
                await ReplyAsync("Event Date must be a valid date mm/dd/yy hh:mm...");
                return;
            }
            await eventCreator.CreateEvent(Context, Context.User as IGuildUser, eventName, eventTime, raidType);
        }
    }
}
