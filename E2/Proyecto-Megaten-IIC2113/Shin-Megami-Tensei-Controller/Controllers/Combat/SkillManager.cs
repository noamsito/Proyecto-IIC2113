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
    
    public static void HandleSpecialSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        string name = skillCtx.Skill.Name;

        switch (name)
        {
            case "Sabbatma":
                SpecialSkillManager.UseSpecialSkill(skillCtx);
                break;

            default:
                break;
        }

        ConsumeMP(skillCtx.Caster, skillCtx.Skill.Cost);
        TurnManager.ManageTurnsForInvocationSkill(turnCtx);
    }

    public static void HandleHealSkills(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        string skillName = skillCtx.Skill.Name;
        Skill skill = skillCtx.Skill;
        int numberHits = CalculateNumberHits(skill.Hits, turnCtx.Attacker);
        int stat = AffinityEffectManager.GetStatForSkill(skillCtx);
        
        double baseDamage = AffinityEffectManager.CalculateBaseDamage(stat, skill.Power);
        var affinityCtx = AffinityEffectManager.CreateAffinityContext(skillCtx, baseDamage);
        
        switch (skillName)
        {
            case "Invitation":
                SummonManager.SummonBySkillInvitation(skillCtx.Attacker);
                
                break;

            default:
                for (int i = 0; i < numberHits; i++)
                {
                    AffinityEffectManager.ApplyHeal(skillCtx, affinityCtx);
                }
                break;
        }
        
        ConsumeMP(skillCtx.Caster, skill.Cost);
        TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        CombatUI.DisplayCombatUi(skillCtx, affinityCtx, numberHits);
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
        
        return hits;
    }

    public static double CalculateHeal(Unit targetUnit, SkillUseContext skillCtx)
    {
        Skill currentSkill = skillCtx.Skill;
        int skillPower = currentSkill.Power;
        int baseHealth = targetUnit.GetBaseStats().GetStatByName("HP");
        
        return Math.Floor((skillPower / 100.0) * baseHealth);
    }
    
    
}
