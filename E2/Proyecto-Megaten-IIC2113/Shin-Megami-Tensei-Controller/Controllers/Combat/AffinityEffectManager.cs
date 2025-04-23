using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class AffinityEffectManager
{
    public static int ApplyAffinityEffect(AffinityContext ctx, TurnContext turnCtx)
    {
        string affinity = AffinityResolver.GetAffinity(ctx.Target, ctx.AttackType);
        
        CombatUI.DisplayAffinityMessage(ctx);

        switch (affinity)
        {
            case "Wk":
                return ctx.BaseDamage * 2;

            case "Rs":
                return ctx.BaseDamage / 2;

            case "Nu":
                return 0;

            case "Dr":
                UnitActionManager.Heal(ctx.BaseDamage);
                return 0;

            case "Rp":
                UnitActionManager.ApplyDamageTaken(ctx.Target, ctx.BaseDamage);
                return 0;

            default:
                return ctx.BaseDamage;
        }
    }

}
