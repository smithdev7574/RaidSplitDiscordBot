using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Infrastructure
{
    public class Logger : ILogger
    {
        private readonly IRaidSplitConfiguration config;

        public Logger(IRaidSplitConfiguration config)
        {
            this.config = config;
        }

        public void DebugLog(string message)
        {
            if (!config.Settings.ShouldShowDebugLog)
                return;

            Console.WriteLine($"{DateTime.Now} - message");
        }
    }
}
