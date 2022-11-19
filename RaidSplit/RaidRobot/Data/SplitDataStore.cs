using Newtonsoft.Json;
using RaidRobot.Data.Entities;
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

        public ConcurrentDictionary<string, GuildMember> Roster => splitData.Roster;
        public ConcurrentDictionary<Guid, RaidEvent> Events => splitData.Events;
        public ConcurrentDictionary<ulong, UnknownMessage> UnknownMessages => splitData.UnknownMessages;


        public SplitDataStore()
        {
            fileName = getFileName();
            splitData = loadFromDataStore();
        }

        private string getFileName()
        {
            string directory = $"{Directory.GetCurrentDirectory()}\\DataFiles";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string fileName = $"{directory}\\SplitData.json";
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
