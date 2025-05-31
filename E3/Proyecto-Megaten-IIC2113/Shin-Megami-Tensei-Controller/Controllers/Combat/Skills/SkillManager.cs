using System.Text.RegularExpressions;
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class SkillManager
{
    public static Skill? SelectSkill(Unit unit)
    {
        var skillListsWithValidMP = GetSkillsWithValidMP(unit);
        CombatUI.DisplaySkills(skillListsWithValidMP);

        string input = CombatUI.GetUserInputWithSeparator();
        int selected = GetSelectedSkillIndex(input);
    
        return ValidateAndSelectSkill(skillListsWithValidMP, selected, unit);
    }
    
    private static List<Skill> GetSkillsWithValidMP(Unit unit)
    {
        return unit.GetSkills()
            .Where(skill => skill.Cost <= unit.GetCurrentStats().GetStatByName("MP"))
            .ToList();
    }
    
    private static int GetSelectedSkillIndex(string input)
    {
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
        if (IsSkillMultiTarget(skillCtx.Skill))
        {
            return HealSkillsManager.HandleMultiTargetHealSkill(skillCtx, turnCtx);
        }

        return HealSkillsManager.HandleSingleTargetHealSkill(skillCtx, turnCtx);
    }

    public static bool HandleDamageSkills(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        if (IsSkillMultiTarget(skillCtx.Skill))
        {
            return DamageSkillsManager.HandleMultiTargetDamageSkill(skillCtx, turnCtx);
        }

        return DamageSkillsManager.HandleSingleTargetDamageSkill(skillCtx, turnCtx);
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
    
    public static bool IsSkillMultiTarget(Skill skill)
    {
        return GameConstants._stringForMultiTarget.Contains(skill.Target);
    }
    
    public static List<Unit> GetTargetsForMultiTargetSkill(SkillUseContext skillCtx)
    {
        List<Unit> targets = new List<Unit>();
        
        ManageIfTargetsAreAlliesOrEnemies(skillCtx, ref targets);
        
        return targets;
    }
    
    
    private static void ManageIfTargetsAreAlliesOrEnemies(SkillUseContext skillCtx, ref List<Unit> targets)
    {
        Skill skill = skillCtx.Skill;
        bool isHealSkill = skill.Type == "Heal";
        
        switch (skill.Target)
        {
            case "Party":
                if (isHealSkill)
                    AddAllAlliesUnitsToTargets(skillCtx, ref targets);
                break;
            case "All":
                if (isHealSkill)
                {
                    AddAllAlliesUnitsToTargets(skillCtx, ref targets);
                }
                else
                {
                    AddAllEnemiesUnitsToTargets(skillCtx, ref targets);
                }
                break;
        }

    }
    
    private static void AddAllAlliesUnitsToTargets(SkillUseContext skillCtx, ref List<Unit> targets)
    {
        PlayerUnitManager playerUnitManaer = skillCtx.Attacker.UnitManager;
        Unit caster = skillCtx.Caster;
        Skill skill = skillCtx.Skill;
        
        foreach (var unit in playerUnitManaer.GetActiveUnits())
        {
            if (unit != null && unit != caster)
            {
                targets.Add(unit);
            }
        }
        
        foreach (var unit in playerUnitManaer.GetReservedUnits())
        {
            if (unit != null)
            {
                targets.Add(unit);
            }
        }
        
        
        if (!GameConstants._healsThatDontApplyToCaster.Contains(skill.Name)) targets.Add(caster);
    }
    
    private static void AddAllEnemiesUnitsToTargets(SkillUseContext skillCtx, ref List<Unit> targets)
    {
        Player defenderPlayer = skillCtx.Defender;
        PlayerUnitManager unitManagerDefender = defenderPlayer.UnitManager;
        
        foreach (var unit in unitManagerDefender.GetActiveUnits())
        {
            if (unit != null && unit.IsAlive())
            {
                targets.Add(unit);
            }
        }
        
        foreach (var unit in unitManagerDefender.GetReservedUnits())
        {
            if (unit != null && unit.IsAlive())
            {
                targets.Add(unit);
            }
        }
    }
}
