using System.Text.RegularExpressions;
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class SkillManager
{
    public static Skill? SelectSkill(View view, Unit unit)
    {
        var skills = unit.GetSkills();
        int displayIndex = 1;

        foreach (var skill in skills)
        {
            if (skill.Cost <= unit.GetCurrentStats().GetStatByName("MP"))
            {
                view.WriteLine($"{displayIndex}-{skill.Name} MP:{skill.Cost}");
                displayIndex++;
            }
        }

        view.WriteLine($"{displayIndex}-Cancelar");
        string input = view.ReadLine();
        int selected = Convert.ToInt32(input) - 1;
        
        view.WriteLine(GameConstants.Separator);

        Stat currentStatUnit = unit.GetCurrentStats();
        if (selected < 0 || selected >= skills.Count || skills[selected].Cost > currentStatUnit.GetStatByName("MP"))
            return null;

        return skills[selected];
    }
    
    public static void ConsumeMP(Unit caster, int cost)
    {
        int current = caster.GetCurrentStats().GetStatByName("MP");
        caster.GetCurrentStats().SetStatByName("MP", Math.Max(0, current - cost));
    }

    public static int CalculateNumberHits(string hitsString, Player attackerPlayer)
    {
        var match = Regex.Match(hitsString, @"(\d+)-(\d+)");

        int hits;
        
        if (match.Success)
        {
            int A = int.Parse(match.Groups[1].Value);
            int B = int.Parse(match.Groups[2].Value);

            int k = attackerPlayer.GetConstantKPlayer();
            int offset = k % (B - A + 1);

            hits = offset + A;
        }
        else
        {
            hits = Convert.ToInt32(hitsString);
        }
        
        Console.WriteLine(hits);

        return hits;
    }
    
}
