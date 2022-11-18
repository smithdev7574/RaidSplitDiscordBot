using Microsoft.Extensions.Configuration;
using RaidRobot.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RaidRobot.Infrastructure
{
    public class Configuration : IRaidSplitConfiguration
    {
        private Settings settings;
        private List<RaidType> raidTypes;
        public Settings Settings => settings;
        public List<RaidType> RaidTypes => raidTypes;


        public Configuration()
        {
            Console.WriteLine("Loading Configuration...");
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false);

            var config = builder.Build();
            settings = config.GetSection("Settings").Get<Settings>();
            raidTypes = config.GetSection("RaidTypes").Get<List<RaidType>>();

            if (!raidTypes.Any())
                Console.WriteLine("***WARNING*** You have no raid types in your appsettings.json file. look at the readme for help.");

            Console.WriteLine($"Configuration Loaded.  Token: {settings.Token}, Index: {settings.Index}, BackupPath: {settings.BackupPath}");
        }
    }
}
