using RaidRobot.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Models
{
    public class UnknownUser
    {
        public ulong UserID { get; set; }
        public string UserName { get; set; }
        public string characterType { get; set; }
    }
}
