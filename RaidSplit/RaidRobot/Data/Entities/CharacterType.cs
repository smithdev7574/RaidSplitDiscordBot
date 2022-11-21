using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Data.Entities
{
    public class CharacterType
    {
        public string Name { get; set; }
        public string EmojiCode { get; set; }
        public bool IsBox { get; set; }
        public decimal CharacterWeight { get; set; }
    }
}
