using RaidRobot.Data.Entities;

namespace RaidRobot.Logic
{
    public interface IGuildMemberConverter
    {
        SplitAttendee ConvertToAttendee(GuildMember member, CharacterType characterType, ulong userID);
    }
}