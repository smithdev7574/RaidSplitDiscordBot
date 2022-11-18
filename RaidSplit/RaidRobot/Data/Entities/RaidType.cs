using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Data.Entities
{
    public class RaidType
    {
        public string Name { get; set; }
        public List<CharacterType> CharacterTypes { get; set; }
    }
}
