using Shin_Megami_Tensei.String_Handlers;

namespace Shin_Megami_Tensei;

public static class TeamBuilder
{
    public static Team BuildTeamFromStringList(List<string> teamUnitDescriptions)
    {
        Team team = new Team();

        foreach (var unitLine in teamUnitDescriptions)
        {
            if (StringHelper.IsSamuraiUnit(unitLine))
            {
                TryAddSamurai(unitLine, team);
            }
            else
            {
                AddDemon(unitLine, team);
            }
        }

        return team;
    }

    private static void TryAddSamurai(string line, Team team)
    {
        if (team.HasSamurai())
        {
            team.SetSamuraiRepeated();
            return;
        }

        var (name, skills) = StringHelper.ExtractSamuraiNameAndSkills(line);
        Samurai samurai = new(name, skills);
        team.AddSamurai(samurai);
    }

    private static void AddDemon(string line, Team team)
    {
        string demonName = line.Trim();
        Demon demon = new(demonName);
        team.AddDemon(demon);
    }
}