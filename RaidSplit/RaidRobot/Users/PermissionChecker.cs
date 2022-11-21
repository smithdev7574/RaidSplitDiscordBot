using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Users
{
    public class PermissionChecker : IPermissionChecker
    {
        public PermissionChecker()
        {

        }


        public (bool HasPremission, string Message) CheckManagerPermissions(SocketCommandContext context, string command)
        {

            var user = context.User as IGuildUser;
            if (user == null) return (false, "Could not find user.");

            if (!user.GuildPermissions.ManageGuild)
            {
                return (false, $"You must be at least a manager within the guild to perform {command}");
            }

            return (true, string.Empty);
        }
    }
}
