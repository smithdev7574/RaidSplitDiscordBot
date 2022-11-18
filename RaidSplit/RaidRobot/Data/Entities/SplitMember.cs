using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Data.Entities
{
    public class SplitMember
    {
        public string CharacterName { get; set; }
        public string ClassName { get; set; }
        public bool IsBox { get; set; }
        public bool IsBoxing { get; set; }
        public string BuddieGroup { get; set; }
        public ulong UserID { get; set; }
        public string SplitReason { get; set; }
        public int? SplitNumber { get; set; }
        public string Rank { get; set; }
        public string Comment { get; set; }
        public decimal Weight { get; set; }
        public int RandomOrder { get; set; }
        public bool IsLate { get; set; }

        public override string ToString()
        {
            if (IsBox)
                return $"{CharacterName} (box)";
            else
                return CharacterName;
        }

        public SplitMember Clone()
        {
            return new SplitMember()
            {
                CharacterName = this.CharacterName,
                ClassName = this.ClassName,
                IsBox = this.IsBox,
                IsBoxing = this.IsBoxing,
                BuddieGroup = this.BuddieGroup,
                UserID = this.UserID,
                Rank = this.Rank,
                Comment = this.Comment,
                Weight = this.Weight,
                RandomOrder = this.RandomOrder,
            };
        }
    }
}
