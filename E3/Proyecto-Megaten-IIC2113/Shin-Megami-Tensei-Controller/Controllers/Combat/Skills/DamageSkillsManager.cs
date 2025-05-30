using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public class DamageSkillsManager
{
    public static bool HandleMultiTargetDamageSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        Skill skill = skillCtx.Skill;
        
        switch (skill.Target)
        {
            case "All":
                MultiTargetSkillManager.HandleMultiTargetOffensiveSkill(skillCtx, turnCtx);
                break;
                
            case "Multi":
                MultiTargetSkillManager.HandleMultiTargetSkill(skillCtx, turnCtx);
                break;
                
            default:
                return HandleMultiTargetDamageSkillLegacy(skillCtx, turnCtx);
        }
        
        return true;
    }

    private static bool HandleMultiTargetDamageSkillLegacy(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        List<Unit> targets = TurnManager.GetTargetsForMultiTargetSkill(skillCtx);
        
        foreach (Unit target in targets)
        { 
            AffinityEffectManager.ApplyEffectForMultiTargetSkill(skillCtx, turnCtx, target);
        }
        
        if (targets.Count > 0)
        {
            Unit targetWithHighestPriority = AffinityEffectManager.GetTargetWithHighestPriorityAffinity(skillCtx, targets);
            
            int stat = AffinityEffectManager.GetStatForSkill(skillCtx);
            double baseDamage = AffinityEffectManager.CalculateBaseDamage(stat, skillCtx.Skill.Power);
            var affinityCtx = new AffinityContext(skillCtx.Caster, targetWithHighestPriority, skillCtx.Skill.Type, baseDamage);
            
            TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        }
        
        CombatUI.DisplaySeparator();
        
        return true;
    }
    
    public static bool HandleSingleTargetDamageSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        AffinityEffectManager.ApplyEffectForSingleTargetSkill(skillCtx, turnCtx);
        return true;
    }
}