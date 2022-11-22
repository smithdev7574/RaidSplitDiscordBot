using Newtonsoft.Json;
using RaidRobot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Infrastructure
{
    internal static class ObjectCloner
    {
        public static T Clone<T>(this T obj) where T : class
        {
            var jsonBlob = JsonConvert.SerializeObject(obj);
            var newObj = JsonConvert.DeserializeObject<T>(jsonBlob);
            return newObj;
        }
    }
}
