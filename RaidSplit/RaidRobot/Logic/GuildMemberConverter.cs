using RaidRobot.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public class GuildMemberConverter : IGuildMemberConverter
    {
        private readonly IRandomizer randomizer;

        public GuildMemberConverter(IRandomizer randomizer)
        {
            this.randomizer = randomizer;
        }

        public SplitAttendee ConvertToAttendee(GuildMember member, CharacterType characterType, ulong userID)
        {
            return new SplitAttendee()
            {
                CharacterName = member.CharacterName,
                ClassName = member.ClassName,
                CanBeLeader = member.CanBeLeader,
                CanBeMasterLooter = member.CanBeMasterLooter,
                CanBeInviter = member.CanBeInviter,
                IsAnchor = member.IsAnchor,
                IsBox = characterType.IsBox,
                BuddieGroup = member.BuddieGroup,
                UserID = userID,
                Rank = member.Rank,
                RandomOrder = randomizer.GetRandomNumber(0, 10000),
                Weight = characterType.CharacterWeight,
            };
        }
    }
}
