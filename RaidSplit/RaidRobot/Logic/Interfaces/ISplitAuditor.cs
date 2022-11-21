using RaidRobot.Data.Entities;
using System.Threading.Tasks;

namespace RaidRobot.Logic.Interfaces
{
    public interface ISplitAuditor
    {
        Task GenerateAuditFile(RaidEvent raidEvent, Split split);
    }
}