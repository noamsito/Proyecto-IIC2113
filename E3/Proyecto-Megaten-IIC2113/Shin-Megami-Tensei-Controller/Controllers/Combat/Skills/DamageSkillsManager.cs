using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public class DamageSkillsManager
{
    public static bool HandleMultiTargetDamageSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        List<Unit> targets = TurnManager.GetTargetsForMultiTargetSkill(skillCtx);
        
        foreach (Unit target in targets)
        { 
            AffinityEffectManager.ApplyEffectForMultiTargetSkill(skillCtx, turnCtx, target);
        }
        
        return true;
    }

    public static bool HandleSingleTargetDamageSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        AffinityEffectManager.ApplyEffectForSingleTargetSkill(skillCtx, turnCtx);
        return true;
    }
}