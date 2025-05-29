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
            Unit targetWithHighestPriority = GetTargetWithHighestPriorityAffinity(skillCtx, targets);
            
            int stat = AffinityEffectManager.GetStatForSkill(skillCtx);
            double baseDamage = AffinityEffectManager.CalculateBaseDamage(stat, skillCtx.Skill.Power);
            var affinityCtx = new AffinityContext(skillCtx.Caster, targetWithHighestPriority, skillCtx.Skill.Type, baseDamage);
            
            TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        }
        
        CombatUI.DisplaySeparator();
        
        return true;
    }

    private static Unit GetTargetWithHighestPriorityAffinity(
        SkillUseContext skillCtx, List<Unit> targets)
    {
        Unit repelOrDrainTarget = null;
        Unit nullTarget        = null;
        Unit missTarget        = null;
        Unit resistTarget      = null;
        Unit weakTarget        = null;

        foreach (Unit target in targets)
        {
            string affinity = AffinityResolver.GetAffinity(target,
                skillCtx.Skill.Type);

            switch (affinity)
            {
                case "Rp":
                case "Dr":
                    if (repelOrDrainTarget == null) repelOrDrainTarget = target;
                    break;

                case "Nu":
                    if (nullTarget == null) nullTarget = target;
                    break;

                case "Miss":
                    if (missTarget == null) missTarget = target;
                    break;

                case "Rs":
                case "-":
                    if (resistTarget == null) resistTarget = target;
                    break;

                case "Wk":
                    if (weakTarget == null) weakTarget = target;
                    break;
            }
        }

        if (repelOrDrainTarget != null) return repelOrDrainTarget;
        if (nullTarget   != null) return nullTarget;
        if (missTarget   != null) return missTarget;
        if (weakTarget   != null) return weakTarget;
        if (resistTarget != null) return resistTarget;

        return targets[0];
    }

    public static bool HandleSingleTargetDamageSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        AffinityEffectManager.ApplyEffectForSingleTargetSkill(skillCtx, turnCtx);
        return true;
    }
}