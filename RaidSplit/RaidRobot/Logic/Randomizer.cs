using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidRobot.Logic
{
    public class Randomizer : IRandomizer
    {
        private readonly Random random;

        public Randomizer()
        {
            this.random = new Random();
        }

        public int GetRandomNumber(int min, int max)
        {
            return random.Next(min, max);
        }
    }
}
