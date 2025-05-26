using System.Text.RegularExpressions;
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class SkillManager
{
    public static Skill? SelectSkill(View view, Unit unit)
    {
        var skillListsWithValidMP = GetSkillsWithValidMP(unit);
        DisplaySkills(view, skillListsWithValidMP);
    
        int selected = GetSelectedSkillIndex(view);
    
        return ValidateAndSelectSkill(skillListsWithValidMP, selected, unit);
    }
    
    private static List<Skill> GetSkillsWithValidMP(Unit unit)
    {
        return unit.GetSkills()
            .Where(skill => skill.Cost <= unit.GetCurrentStats().GetStatByName("MP"))
            .ToList();
    }
    
    private static void DisplaySkills(View view, List<Skill> skills)
    {
        int displayIndex = 1;
    
        foreach (var skill in skills)
        {
            view.WriteLine($"{displayIndex}-{skill.Name} MP:{skill.Cost}");
            displayIndex++;
        }
    
        view.WriteLine($"{displayIndex}-Cancelar");
    }
    
    private static int GetSelectedSkillIndex(View view)
    {
        string input = view.ReadLine();
        view.WriteLine(GameConstants.Separator);
        return Convert.ToInt32(input) - 1;
    }
    
    private static Skill? ValidateAndSelectSkill(List<Skill> skills, int selected, Unit unit)
    {
        Stat currentStatUnit = unit.GetCurrentStats();
    
        if (selected < 0 || selected >= skills.Count || 
            skills[selected].Cost > currentStatUnit.GetStatByName("MP"))
            return null;
    
        return skills[selected];
    }
    
    public static bool HandleSpecialSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        string name = skillCtx.Skill.Name;
        bool usedSkill = true;
        
        switch (name)
        {
            case "Sabbatma":
                usedSkill = SpecialSkillManager.UseSpecialSkill(skillCtx);
                break;
        }

        if (usedSkill)
        {
            ConsumeMP(skillCtx.Caster, skillCtx.Skill.Cost);
            TurnManager.UpdateTurnsForInvocationSkill(turnCtx);
        }

        return usedSkill;
    }

    public static bool HandleHealSkills(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        if (HealSkillsManager.IsMultiTargetHealSkill(skillCtx.Skill))
        {
            return HealSkillsManager.HandleMultiTargetHealSkill(skillCtx, turnCtx);
        }

        return HealSkillsManager.HandleSingleTargetHealSkill(skillCtx, turnCtx);
    }
    
    public static void ConsumeMP(Unit caster, int cost)
    {
        int current = caster.GetCurrentStats().GetStatByName("MP");
        caster.GetCurrentStats().SetStatByName("MP", Math.Max(0, current - cost));
    }

    public static int CalculateNumberHits(string hitsString, Player attackerPlayer)
    {
        PlayerTurnManager playerUnitManager = attackerPlayer.TurnManager;
        var match = Regex.Match(hitsString, @"(\d+)-(\d+)");
        int hits;

        if (match.Success)
        {
            int A = int.Parse(match.Groups[1].Value);
            int B = int.Parse(match.Groups[2].Value);

            int k = playerUnitManager.GetConstantKPlayer();
            int offset = k % (B - A + 1);

            hits = offset + A;
        }
        else
        {
            hits = Convert.ToInt32(hitsString);
        }
        
        return hits;
    }
}
