using Discord.Commands;

namespace RaidRobot.Users
{
    public interface IPermissionChecker
    {
        (bool HasPremission, string Message) CheckManagerPermissions(SocketCommandContext context, string command);
    }
}