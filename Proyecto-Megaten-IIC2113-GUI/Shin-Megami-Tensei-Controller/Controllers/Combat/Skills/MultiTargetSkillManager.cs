using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Data;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class MultiTargetSkillManager
{
    private const string LIGHT_SKILL_TYPE = "Light";
    private const string DARK_SKILL_TYPE = "Dark";

    public static void HandleMultiTargetOffensiveSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        var targets = GetTargetsInCorrectOrder(skillCtx, turnCtx);
        var skillExecutionData = CreateSkillExecutionData(skillCtx, turnCtx);
        
        if (ShouldSkipExecution(targets)) return;

        var hitResults = ExecuteHitsOnTargets(skillExecutionData, targets);
        ProcessRepelDamageIfNeeded(skillExecutionData, hitResults);
        ProcessTurnConsumption(skillExecutionData, targets, hitResults);
        CombatUI.DisplaySeparator();
    }

    public static void HandleMultiTargetSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        var targets = GetMultiTargetsUsingAlgorithm(skillCtx, turnCtx);
        var skillExecutionData = CreateSkillExecutionData(skillCtx, turnCtx);
        
        if (ShouldSkipExecution(targets)) return;

        var hitsDistribution = CalculateHitsDistribution(skillExecutionData.NumberOfHits, targets);
        var hitResults = ExecuteDistributedHitsOnTargets(skillExecutionData, hitsDistribution);
        ProcessRepelDamageIfNeeded(skillExecutionData, hitResults);
        ProcessTurnConsumption(skillExecutionData, targets, hitResults);
        CombatUI.DisplaySeparator();
    }

    private static SkillExecutionData CreateSkillExecutionData(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        var numberOfHits = SkillManager.CalculateNumberHits(skillCtx.Skill.Hits, turnCtx.Attacker);
        return new SkillExecutionData(skillCtx, turnCtx, numberOfHits);
    }

    private static bool ShouldSkipExecution(List<Unit> targets)
    {
        return targets.Count == 0;
    }

    private static HitExecutionResults ExecuteHitsOnTargets(SkillExecutionData executionData, List<Unit> targets)
    {
        var hitResults = new HitExecutionResults();

        foreach (Unit target in targets)
        {
            var targetSpecificContext = CreateTargetSpecificContext(executionData, target);
            ExecuteMultipleHitsOnSingleTarget(targetSpecificContext, executionData.NumberOfHits, hitResults);
        }

        return hitResults;
    }

    private static Dictionary<Unit, int> CalculateHitsDistribution(int totalHits, List<Unit> targets)
    {
        var hitsPerTarget = new Dictionary<Unit, int>();
        
        for (int hitIndex = 0; hitIndex < totalHits; hitIndex++)
        {
            Unit targetForThisHit = targets[hitIndex % targets.Count];
            if (!hitsPerTarget.ContainsKey(targetForThisHit))
                hitsPerTarget[targetForThisHit] = 0;
            hitsPerTarget[targetForThisHit]++;
        }

        return hitsPerTarget;
    }

    private static HitExecutionResults ExecuteDistributedHitsOnTargets(SkillExecutionData executionData, Dictionary<Unit, int> hitsDistribution)
    {
        var hitResults = new HitExecutionResults();

        foreach (var targetHitPair in hitsDistribution)
        {
            Unit target = targetHitPair.Key;
            int hitsForThisTarget = targetHitPair.Value;
            
            var targetSpecificContext = CreateTargetSpecificContext(executionData, target);
            ExecuteMultipleHitsOnSingleTarget(targetSpecificContext, hitsForThisTarget, hitResults);
        }

        return hitResults;
    }

    private static SkillUseContext CreateTargetSpecificContext(SkillExecutionData executionData, Unit target)
    {
        return SkillUseContext.CreateSkillContext(
            executionData.SkillContext.Caster, 
            target, 
            executionData.Skill, 
            executionData.TurnContext
        );
    }

    private static void ExecuteMultipleHitsOnSingleTarget(SkillUseContext targetContext, int numberOfHits, HitExecutionResults hitResults)
    {
        for (int hitIndex = 0; hitIndex < numberOfHits; hitIndex++)
        {
            var affinityContext = ProcessSingleTargetHit(targetContext, out bool isRepelHit, out double repelDamageAmount);
            hitResults.AllAffinityContexts.Add(affinityContext);
            
            if (isRepelHit)
            {
                hitResults.RepelTargets.Add(targetContext.Target);
                hitResults.TotalRepelDamage += repelDamageAmount;
            }
        }
    }

    private static void ProcessRepelDamageIfNeeded(SkillExecutionData executionData, HitExecutionResults hitResults)
    {
        if (HasRepelTargetsAndNotLightDarkSkill(hitResults.RepelTargets, executionData.Skill))
        {
            ApplyRepelDamageToCaster(executionData.SkillContext.Caster, hitResults.TotalRepelDamage);
        }
    }

    private static bool HasRepelTargetsAndNotLightDarkSkill(List<Unit> repelTargets, Skill skill)
    {
        return repelTargets.Count > 0 && !IsLightOrDarkSkill(skill);
    }

    private static bool IsLightOrDarkSkill(Skill skill)
    {
        return skill.Type == LIGHT_SKILL_TYPE || skill.Type == DARK_SKILL_TYPE;
    }

    private static void ApplyRepelDamageToCaster(Unit caster, double totalRepelDamage)
    {
        UnitActionManager.ApplyDamageTaken(caster, totalRepelDamage);
        CombatUI.DisplayFinalHP(caster);
    }

    private static void ProcessTurnConsumption(SkillExecutionData executionData, List<Unit> targets, HitExecutionResults hitResults)
    {
        if (ShouldProcessTurnConsumption(hitResults.AllAffinityContexts))
        {
            var turnAffinityContext = CreateTurnAffinityContext(executionData, targets);
            TurnManager.ConsumeTurnsBasedOnAffinity(turnAffinityContext, executionData.TurnContext);
        }
    }

    private static bool ShouldProcessTurnConsumption(List<AffinityContext> allAffinityContexts)
    {
        return allAffinityContexts.Count > 0;
    }

    private static AffinityContext CreateTurnAffinityContext(SkillExecutionData executionData, List<Unit> targets)
    {
        var targetWithHighestPriority = AffinityEffectManager.GetTargetWithHighestPriorityAffinity(executionData.SkillContext, targets);
        var casterStat = AffinityEffectManager.GetStatForSkill(executionData.SkillContext);
        var baseDamageAmount = AffinityEffectManager.CalculateBaseDamage(casterStat, executionData.Skill.Power);
        
        return new AffinityContext(executionData.SkillContext.Caster, targetWithHighestPriority, executionData.Skill.Type, baseDamageAmount);
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
        
        return orderedTargets.Where(unit => ShouldReceiveEffect(unit)).ToList();
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

    private static bool ShouldReceiveEffect(Unit unit)
    {
        return unit.IsAlive();
    }
}