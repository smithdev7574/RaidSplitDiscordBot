using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Data.Entities
{
    public class GuildMember
    {
        public string CharacterName { get; set; }
        public int Level { get; set; }
        public string ClassName { get; set; }
        public string Rank { get; set; }
        public string Comment { get; set; }
        public ulong? UserId { get; set; }
        public string BuddieGroup { get; set; }
        public string CharacterType { get; set; }
        public bool IsAnchor { get; set; }
        public bool CanBeLeader { get; set; }
        public bool CanBeMasterLooter { get; set; }
        public bool CanBeInviter { get; set; }
    }
}
