using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Data.Entities
{
    public class UnknownMessage
    {
        public ulong MessageID { get; set; }
        public ulong ChannelID { get; set; }
        public ulong UserID { get; set; }
        public ulong GuildID { get; set; }
        public string CharacterType { get; set; }
        public string UserName { get;  set; }
        public UnknownMessageTypes MessageType { get; set; }
        public string CharacterName { get; set; }
    }
}
