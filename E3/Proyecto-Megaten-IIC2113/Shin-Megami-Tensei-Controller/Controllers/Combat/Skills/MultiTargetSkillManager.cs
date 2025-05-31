using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class MultiTargetSkillManager
{
    public static void HandleMultiTargetOffensiveSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        Skill skill = skillCtx.Skill;
        List<Unit> targets = GetTargetsInCorrectOrder(skillCtx, turnCtx);
        
        if (targets.Count == 0) return;

        int numHits = SkillManager.CalculateNumberHits(skill.Hits, turnCtx.Attacker);
        List<AffinityContext> allAffinityContexts = new List<AffinityContext>();
        List<Unit> repelTargets = new List<Unit>();
        double totalRepelDamage = 0;
        Unit lastRepelTarget = null;

        foreach (Unit target in targets)
        {
            var specificSkillCtx = SkillUseContext.CreateSkillContext(skillCtx.Caster, target, skill, turnCtx);
            
            for (int hit = 0; hit < numHits; hit++)
            {
                var affinityCtx = ProcessSingleTargetHit(specificSkillCtx, out bool isRepel, out double repelDamage);
                allAffinityContexts.Add(affinityCtx);
                
                if (isRepel)
                {
                    repelTargets.Add(target);
                    totalRepelDamage += repelDamage;
                }
            }
        }

        if (repelTargets.Count > 0 && (skill.Type != "Light" && skill.Type != "Dark"))
        {
            UnitActionManager.ApplyDamageTaken(skillCtx.Caster, totalRepelDamage);
            CombatUI.DisplayFinalHP(skillCtx.Caster);
        }

        if (allAffinityContexts.Count > 0)
        {
            Unit targetWithHighestPriority = AffinityEffectManager.GetTargetWithHighestPriorityAffinity(skillCtx, targets);
            int stat = AffinityEffectManager.GetStatForSkill(skillCtx);
            double baseDamage = AffinityEffectManager.CalculateBaseDamage(stat, skillCtx.Skill.Power);
            var affinityCtx = new AffinityContext(skillCtx.Caster, targetWithHighestPriority, skillCtx.Skill.Type, baseDamage);
         
            TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        }
        
        CombatUI.DisplaySeparator();
    }

    public static void HandleMultiTargetSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        Skill skill = skillCtx.Skill;
        List<Unit> targets = GetMultiTargetsUsingAlgorithm(skillCtx, turnCtx);
        
        if (targets.Count == 0) return;

        int numHits = SkillManager.CalculateNumberHits(skill.Hits, turnCtx.Attacker);
        List<AffinityContext> allAffinityContexts = new List<AffinityContext>();
        List<Unit> repelTargets = new List<Unit>();
        double totalRepelDamage = 0;

        var hitsPerTarget = new Dictionary<Unit, int>();
        for (int hit = 0; hit < numHits; hit++)
        {
            Unit target = targets[hit % targets.Count];
            if (!hitsPerTarget.ContainsKey(target))
                hitsPerTarget[target] = 0;
            hitsPerTarget[target]++;
        }

        foreach (var kvp in hitsPerTarget)
        {
            Unit target = kvp.Key;
            int hits = kvp.Value;
            
            var specificSkillCtx = SkillUseContext.CreateSkillContext(skillCtx.Caster, target, skill, turnCtx);
            
            for (int hit = 0; hit < hits; hit++)
            {
                var affinityCtx = ProcessSingleTargetHit(specificSkillCtx, out bool isRepel, out double repelDamage);
                allAffinityContexts.Add(affinityCtx);
                
                if (isRepel)
                {
                    repelTargets.Add(target);
                    totalRepelDamage += repelDamage;
                }
            }
        }

        if (repelTargets.Count > 0 && (skill.Type != "Light" && skill.Type != "Dark"))
        {   
            UnitActionManager.ApplyDamageTaken(skillCtx.Caster, totalRepelDamage);
            CombatUI.DisplayFinalHP(skillCtx.Caster);
        }

        if (allAffinityContexts.Count > 0)
        {
            Unit targetWithHighestPriority = AffinityEffectManager.GetTargetWithHighestPriorityAffinity(skillCtx, targets);
            int stat = AffinityEffectManager.GetStatForSkill(skillCtx);
            double baseDamage = AffinityEffectManager.CalculateBaseDamage(stat, skillCtx.Skill.Power);
            var affinityCtx = new AffinityContext(skillCtx.Caster, targetWithHighestPriority, skillCtx.Skill.Type, baseDamage);
         
            TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        }
        
        CombatUI.DisplaySeparator();
    }
    

    private static AffinityContext ProcessSingleTargetHit(SkillUseContext skillCtx, out bool isRepel, out double repelDamage)
    {
        int stat = AffinityEffectManager.GetStatForSkill(skillCtx);
        double baseDamage = AffinityEffectManager.CalculateBaseDamage(stat, skillCtx.Skill.Power);
        var affinityCtx = new AffinityContext(skillCtx.Caster, skillCtx.Target, skillCtx.Skill.Type, baseDamage);
        
        isRepel = false;
        repelDamage = 0;

        DisplaySkillUsage(skillCtx.Caster, skillCtx.Skill, skillCtx.Target);

        if (skillCtx.Skill.Type == "Light" || skillCtx.Skill.Type == "Dark")
        {
            HandleLightDarkSkill(affinityCtx, skillCtx, out isRepel);
        }
        else
        {
            HandleRegularOffensiveSkill(affinityCtx, out isRepel, out repelDamage);
        }
        
        return affinityCtx;
    }

    private static void HandleRegularOffensiveSkill(AffinityContext affinityCtx, out bool isRepel, out double repelDamage)
    {
        string affinity = AffinityResolver.GetAffinity(affinityCtx.Target, affinityCtx.AttackType);
        double finalDamage = AffinityEffectManager.GetDamageBasedOnAffinity(affinityCtx);
        
        isRepel = false;
        repelDamage = 0;

        switch (affinity)
        {
            case "Wk":
                CombatUI.DisplayWeakMessage(affinityCtx.Target, affinityCtx.Caster);
                UnitActionManager.ApplyDamageTaken(affinityCtx.Target, finalDamage);
                CombatUI.DisplayDamageReceived(affinityCtx.Target, (int)Math.Floor(finalDamage));
                break;
                
            case "Rs":
                CombatUI.DisplayResistMessage(affinityCtx.Target, affinityCtx.Caster);
                UnitActionManager.ApplyDamageTaken(affinityCtx.Target, finalDamage);
                CombatUI.DisplayDamageReceived(affinityCtx.Target, (int)Math.Floor(finalDamage));
                break;
                
            case "Nu":
                CombatUI.DisplayBlockMessage(affinityCtx.Target, affinityCtx.Caster);
                break;
                
            case "Dr":
                UnitActionManager.ApplyHealToUnit(affinityCtx.Target, affinityCtx.BaseDamage);
                CombatUI.DisplayDrainMessage(affinityCtx.Target, (int)Math.Floor(affinityCtx.BaseDamage));
                break;
                
            case "Rp":
                isRepel = true;
                repelDamage = Math.Floor(affinityCtx.BaseDamage);
                CombatUI.DisplayRepelMessage(affinityCtx.Target, affinityCtx.Caster, (int)repelDamage);
                break;
                
            default:
                UnitActionManager.ApplyDamageTaken(affinityCtx.Target, finalDamage);
                CombatUI.DisplayDamageReceived(affinityCtx.Target, (int)Math.Floor(finalDamage));
                break;
        }
        
        if (affinity != "Rp") CombatUI.DisplayFinalHP(affinityCtx.Target);
    }
    
    private static void HandleLightDarkSkill(AffinityContext affinityCtx, SkillUseContext skillCtx, out bool isRepel)
    {
        string affinity = AffinityResolver.GetAffinity(affinityCtx.Target, affinityCtx.AttackType);
        int lckCaster = skillCtx.Caster.GetCurrentStats().GetStatByName("Lck");
        int lckTarget = skillCtx.Target.GetCurrentStats().GetStatByName("Lck");
        double skillPower = skillCtx.Skill.Power;
        
        isRepel = false;
        
        switch (affinity)
        {
            case "Wk":
                CombatUI.DisplayWeakMessage(affinityCtx.Target, affinityCtx.Caster);
                KillTarget(affinityCtx.Target);
                CombatUI.DisplayUnitEliminated(affinityCtx.Target);
                break;

            case "Nu":
                CombatUI.DisplayBlockMessage(affinityCtx.Target, affinityCtx.Caster);
                break;

            case "Rs":
                if ((lckCaster + skillPower) >= (2 * lckTarget))
                {
                    CombatUI.DisplayResistMessage(affinityCtx.Target, affinityCtx.Caster);
                    KillTarget(affinityCtx.Target);
                    CombatUI.DisplayUnitEliminated(affinityCtx.Target);
                }
                else
                {
                    CombatUI.DisplayHasMissed(affinityCtx.Caster);
                }
                break;

            case "Rp":
                isRepel = true;
                KillTarget(affinityCtx.Caster);
                CombatUI.DisplayHasMissed(affinityCtx.Caster);
                break;

            case "Dr":
                break;
                
            default:
                if (lckCaster + skillPower >= lckTarget)
                {
                    KillTarget(affinityCtx.Target);
                    CombatUI.DisplayUnitEliminated(affinityCtx.Target);
                }
                else
                {
                    CombatUI.DisplayHasMissed(affinityCtx.Caster);
                }
                break;
        }
        
        if (affinity != "Rp") CombatUI.DisplayFinalHP(affinityCtx.Target);
    }

    private static void KillTarget(Unit target)
    {
        double hpToKill = target.GetCurrentStats().GetStatByName("HP");
        UnitActionManager.ApplyDamageTaken(target, hpToKill);
    }

    private static void DisplaySkillUsage(Unit caster, Skill skill, Unit target)
    {
        CombatUI.DisplaySkillUsage(caster, skill, target);
    }

    private static List<Unit> GetTargetsInCorrectOrder(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        List<Unit> orderedTargets = new List<Unit>();
        
        switch (skillCtx.Skill.Target)
        {
            case "All":
                AddOpponentActiveUnits(skillCtx, orderedTargets);
                break;
                
            case "Multi":
                return GetMultiTargetsUsingAlgorithm(skillCtx, turnCtx);
        }
        
        return orderedTargets.Where(unit => ShouldReceiveEffect(unit, skillCtx.Skill)).ToList();
    }

    private static List<Unit> GetMultiTargetsUsingAlgorithm(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        PlayerUnitManager opponentUnitManager = skillCtx.Defender.UnitManager;
        
        var activeSlots = opponentUnitManager.GetActiveUnits();
        var activeEnemies = activeSlots
            .Where(unit => unit != null && unit.IsAlive())
            .ToList();
        
        if (activeEnemies.Count == 0) return new List<Unit>();

        int K = turnCtx.Attacker.TurnManager.GetConstantKPlayer();
        int A = activeEnemies.Count;
        int hits = SkillManager.CalculateNumberHits(skillCtx.Skill.Hits, turnCtx.Attacker);
        
        int i = K % A;
        bool directionLeft = (i % 2 != 0);
        
        List<Unit> selectedTargets = new List<Unit>();
        int currentIndex = i;
        
        selectedTargets.Add(activeEnemies[currentIndex]);
        
        for (int h = 1; h < hits; h++)
        {
            if (directionLeft)
            {
                currentIndex = (currentIndex - 1 + A) % A;
            }
            else
            {
                currentIndex = (currentIndex + 1) % A;
            }
            
            selectedTargets.Add(activeEnemies[currentIndex]);
        }
        
        return selectedTargets;
    }

    private static void AddOpponentActiveUnits(SkillUseContext skillCtx, List<Unit> targets)
    {
        var activeUnits = skillCtx.Defender.UnitManager.GetActiveUnits();
        foreach (var unit in activeUnits)
        {
            if (unit != null && unit.IsAlive())
            {
                targets.Add(unit);
            }
        }
    }

    private static void AddOpponentReserveUnits(SkillUseContext skillCtx, List<Unit> targets)
    {
        var reserveUnits = skillCtx.Defender.UnitManager.GetReservedUnits();
        foreach (var unit in reserveUnits)
        {
            if (unit != null && unit.IsAlive())
            {
                targets.Add(unit);
            }
        }
    }
    
    private static bool ShouldReceiveEffect(Unit unit, Skill skill)
    {
        return unit.IsAlive();
    }
}