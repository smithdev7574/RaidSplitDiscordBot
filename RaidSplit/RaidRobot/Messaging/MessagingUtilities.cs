using Discord.Rest;
using RaidRobot.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Messaging
{
    public static class MessagingUtilities
    {
        public static MessageDetail ConvertToMessageDetail(this RestUserMessage message)
        {
            return new MessageDetail()
            {
                ChannelID = message.Channel.Id,
                MessageID = message.Id,
            };
        }
    }
}
