using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Data.Entities
{
    public class GameClass
    {
        public string Name { get; set; }
        public string EmojiCode { get; set; }
        public string ShortName { get; set; }
        public bool IsMelee { get; set; }
        public bool IsCaster { get; set; }
    }
}
