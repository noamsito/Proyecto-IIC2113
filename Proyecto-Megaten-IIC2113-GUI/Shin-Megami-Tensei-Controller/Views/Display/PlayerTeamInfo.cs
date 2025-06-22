public class PlayerTeamInfo
{
    public string SamuraiName { get; }
    public List<string> SkillNames { get; }
    public List<string> DemonNames { get; }

    public PlayerTeamInfo(string samuraiName, List<string> skillNames, List<string> demonNames)
    {
        SamuraiName = samuraiName ?? throw new ArgumentNullException(nameof(samuraiName));
        SkillNames = skillNames ?? throw new ArgumentNullException(nameof(skillNames));
        DemonNames = demonNames ?? throw new ArgumentNullException(nameof(demonNames));
    }
}