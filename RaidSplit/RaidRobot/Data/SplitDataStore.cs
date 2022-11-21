using Newtonsoft.Json;
using RaidRobot.Data.Entities;
using RaidRobot.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Data
{
    public class SplitDataStore : ISplitDataStore
    {
        private object lockObj = new object();
        private SplitData splitData;
        string fileName;
        private readonly IRaidSplitConfiguration config;

        public ConcurrentDictionary<string, GuildMember> Roster => splitData.Roster;
        public ConcurrentDictionary<Guid, RaidEvent> Events => splitData.Events;
        public ConcurrentDictionary<ulong, UnknownMessage> UnknownMessages => splitData.UnknownMessages;
        public ConcurrentDictionary<string, PreSplit> PreSplits => splitData.PreSplits;


        public SplitDataStore(IRaidSplitConfiguration config)
        {
            fileName = getFileName();
            splitData = loadFromDataStore();
            this.config = config;
        }

        private string getFileName()
        {
            if(!Directory.Exists(config.Settings.DataFileDirectoryPath))
                Directory.CreateDirectory(config.Settings.DataFileDirectoryPath);

            string fileName = Path.Combine(config.Settings.DataFileDirectoryPath, Constants.DATA_FILE_NAME);
            return fileName;
        }

        private SplitData loadFromDataStore()
        {
            var data = new SplitData();
            if (File.Exists(fileName))
            {
                string jsonBlob = File.ReadAllText(fileName);
                data = JsonConvert.DeserializeObject<SplitData>(jsonBlob);
            }

            return data;
        }

        public string UpdateRoster(List<GuildMember> roster)
        {
            int addedCount = 0;
            int updatedCount = 0;

            StringBuilder added = new StringBuilder();
            StringBuilder updated = new StringBuilder();

            foreach (var member in roster)
            {
                lock (lockObj)
                {
                    splitData.Roster.TryGetValue(member.CharacterName, out var foundMember);
                    if (foundMember == null)
                    {
                        splitData.Roster[member.CharacterName] = member;
                        addedCount++;
                        added.Append($", {member.CharacterName}");
                    }
                    else
                    {
                        if (foundMember.Rank != member.Rank || foundMember.Level != member.Level || foundMember.Comment != member.Comment)
                        {
                            foundMember.Rank = member.Rank;
                            foundMember.Level = member.Level;
                            foundMember.Comment = member.Comment;
                            updatedCount++;
                            updated.Append($", {member.CharacterName}");
                        }
                    }
                }
            }

            SaveChanges();

            return $"**Added ({addedCount})**{Environment.NewLine}**Updated ({updatedCount})**{Environment.NewLine}";
        }

        public void SaveChanges()
        {
            var jsonBlob = JsonConvert.SerializeObject(splitData);
            lock (lockObj)
            {
                File.WriteAllText(fileName, jsonBlob);
            }
        }

    }
}
