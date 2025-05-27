using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public class DamageSkillsManager
{
    public static bool HandleMultiTargetDamageSkill(SkillUseContext skillCtx, TurnContext turnCtx, int numberHits)
    {
        Skill skill = skillCtx.Skill;
        List<Unit> targets = TurnManager.GetTargetsForMultiTargetSkill(skillCtx);
        
        foreach (Unit target in targets)
        { 
            AffinityEffectManager.ApplyEffectForSkill(skillCtx, turnCtx, numberHits);
        }
        
        return true;
    }

    public static bool HandleSingleTargetDamageSkill(SkillUseContext skillCtx, TurnContext turnCtx, int numberHits)
    {
        AffinityEffectManager.ApplyEffectForSkill(skillCtx, turnCtx, numberHits);
        return true;
    }
}