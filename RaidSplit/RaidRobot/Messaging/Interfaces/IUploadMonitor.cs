using Discord.Commands;

namespace RaidRobot.Messaging.Interfaces
{
    public interface IUploadMonitor
    {
        string MonitorRosterUpload(SocketCommandContext context);
    }
}