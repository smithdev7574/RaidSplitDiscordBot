using RaidRobot.Data.Entities;
using System.Collections.Generic;

namespace RaidRobot.Infrastructure
{
    public interface IRaidSplitConfiguration
    {
        Settings Settings { get; }
        List<RaidType> RaidTypes { get; }
    }
}