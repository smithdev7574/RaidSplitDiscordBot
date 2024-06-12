using System;
using System.Collections.Generic;
using System.Text;

namespace RaidRobot.Infrastructure
{
    public class Settings
    {
        public string Token { get; set; }
        public int Index { get; set; }
        public string BackupPath { get; set; }
        public string MessagePrefix { get; set; }
        public string RegistrationChannel { get; set; }
        public string SpamChannel { get; set; }
        public string AdminChannel { get; set; }
        public string SplitChannel { get; set; }
        public string DataFileDirectoryPath { get; set; }
        public string TempFileDirectoryPath { get; set; }
        public bool ShouldShowDebugLog { get; set; }
    }
}
