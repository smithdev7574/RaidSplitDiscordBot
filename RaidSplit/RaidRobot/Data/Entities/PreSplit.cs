using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Data.Entities
{
    public class PreSplit
    {
        public string Name { get; set; }
        public List<string> Characters { get; set; } = new List<string>();
        public string LeaderName { get; set; }
        public string LooterName { get; set; }
        public string InviterName { get; set; }
        public int RandomNumber { get; set; }

        public override string ToString()
        {
            return $"PreSplit **{Name}** - Leader: **{LeaderName ?? "N/A"}**, " +
                $"Looter: **{LooterName ?? "N/A"}**, Inviter: **{InviterName ?? "N/A"}**, Characters: {string.Join(", ", Characters)}.";
        }
    }
}
