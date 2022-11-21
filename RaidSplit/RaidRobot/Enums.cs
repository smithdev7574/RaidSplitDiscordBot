using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot
{
    public enum MessageContexts
    {
        LeaderMessage = 1,
        LooterMessage = 2,
        InviterMessage = 3,
        Announcement = 4,
        Audit = 5,
        FinalAnnouncement = 6,
        ResponsibilityMessage = 7,
        LatePreview = 8,
        LateAnnouncement = 9,
        EveryoneRegistrationMessage = 10,
        RegistrationMessage = 11,
        AttendeeMessage = 12
    }

    public enum UnknownMessageTypes
    {
        Character = 1,
        Class = 2,
    }

    public enum RaidResponsibilities
    {
        Leader = 1,
        Looter = 2,
        Inviter = 3,
    }

}
