namespace RaidRobot.Logic
{
    public interface IPreSplitOrchestrator
    {
        string AddCharacters(string preSplitName, string characterNames);
        string CreatePreSplit(string name, string leader = null, string looter = null, string inviter = null);
        string RemoveCharacters(string preSplitName, string characterNames);
        string RemovePreSplit(string preSplitName);
        string SetRole(string preSplitName, string characterName, RaidResponsibilities responsibility);
        string ViewPreSplits();
    }
}