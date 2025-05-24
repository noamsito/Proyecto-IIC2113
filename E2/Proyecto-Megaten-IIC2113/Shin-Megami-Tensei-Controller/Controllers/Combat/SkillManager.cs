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
        var skillListsWithValidMP = skills
            .Where(skill => skill.Cost <= unit.GetCurrentStats().GetStatByName("MP"))
            .ToList();
        
        int displayIndex = 1;

        foreach (var skill in skillListsWithValidMP)
        {
            view.WriteLine($"{displayIndex}-{skill.Name} MP:{skill.Cost}");
            displayIndex++;
        }

        view.WriteLine($"{displayIndex}-Cancelar"); 
        
        string input = view.ReadLine();
        
        int selected = Convert.ToInt32(input) - 1;
        
        view.WriteLine(GameConstants.Separator);
        
        Stat currentStatUnit = unit.GetCurrentStats();
        if (selected < 0 || selected >= skillListsWithValidMP.Count || 
            skillListsWithValidMP[selected].Cost > currentStatUnit.GetStatByName("MP"))
            return null;

        return skillListsWithValidMP[selected];
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

            default:
                break;
        }

        if (usedSkill)
        {
            ConsumeMP(skillCtx.Caster, skillCtx.Skill.Cost);
            TurnManager.ManageTurnsForInvocationSkill(turnCtx);
        }

        return usedSkill;
    }

    public static bool HandleHealSkills(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        string skillName = skillCtx.Skill.Name;
        Skill skill = skillCtx.Skill;
        int numberHits = CalculateNumberHits(skill.Hits, turnCtx.Attacker);
        int stat = AffinityEffectManager.GetStatForSkill(skillCtx);
        bool hasBeenRevived = false;
        bool usedSkill;
        
        double baseDamage = AffinityEffectManager.CalculateBaseDamage(stat, skill.Power);
        var affinityCtx = AffinityEffectManager.CreateAffinityContext(skillCtx, baseDamage);
        
        switch (skillName)
        {
            case "Invitation":
                affinityCtx.Target = skillCtx.Target;
                usedSkill = SummonManager.SummonBySkillInvitation(skillCtx, turnCtx);
                break;
            
            case "Recarm":
                affinityCtx.Target = skillCtx.Target;
                usedSkill = HandleRecarmSkill(skillCtx);
                
                TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
                TurnManager.UpdateTurnStatesForDisplay(turnCtx);
                turnCtx.Attacker.RearrangeSortedUnitsWhenAttacked();
                
                break;

            default:
                for (int i = 0; i < numberHits; i++)
                {
                    AffinityEffectManager.ApplyHeal(skillCtx);
                }

                usedSkill = true;
                
                CombatUI.DisplayCombatUiForSkill(skillCtx, affinityCtx, numberHits);
                TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
                TurnManager.UpdateTurnStatesForDisplay(turnCtx);
                turnCtx.Attacker.RearrangeSortedUnitsWhenAttacked();
                break;
        }

        if (usedSkill)
        {
            ConsumeMP(skillCtx.Caster, skill.Cost);
        }
        
        return usedSkill;
    }
    
    private static bool HandleRecarmSkill(SkillUseContext skillCtx)
    {
        Player attackerPlayer = skillCtx.Attacker;
        Unit unitCaster = skillCtx.Caster;
        Unit unitTarget = skillCtx.Target;
        Skill skillUsing = skillCtx.Skill;

        List<Unit> activeUnitsList = attackerPlayer.GetActiveUnits();
        
        CombatUI.DisplaySkillUsage(unitCaster, skillUsing, unitTarget);
        double amountHealed = CalculateHalfHp(unitTarget, skillCtx); 
        AffinityEffectManager.ApplyHalfHeal(skillCtx);
        
        if (activeUnitsList.Contains(unitTarget))
        {
            attackerPlayer.AddUnitInSortedList(unitTarget);
        }        
        
        CombatUI.DisplayHealing(unitTarget, amountHealed);
        return true;
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
        
        Console.WriteLine(match.Groups[1].Value);
        Console.WriteLine(match.Groups[2].Value);
        
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
        
        return hits;
    }

    public static double CalculateHeal(Unit targetUnit, SkillUseContext skillCtx)
    {
        Skill currentSkill = skillCtx.Skill;
        int skillPower = currentSkill.Power;
        int baseHealth = targetUnit.GetBaseStats().GetStatByName("HP");
        
        return Math.Floor((skillPower / 100.0) * baseHealth);
    }

    public static double CalculateHalfHp(Unit targetUnit, SkillUseContext skillCtx)
    {
        int baseHealth = targetUnit.GetBaseStats().GetStatByName("HP");
        
        return Math.Floor(baseHealth / 2.0);
    }
}
