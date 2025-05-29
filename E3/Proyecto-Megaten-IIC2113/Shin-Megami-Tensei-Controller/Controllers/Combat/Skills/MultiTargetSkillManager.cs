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

        if (repelTargets.Count > 0)
        {
            UnitActionManager.ApplyDamageTaken(skillCtx.Caster, totalRepelDamage);
            CombatUI.DisplayFinalHP(skillCtx.Caster);
        }

        string highestPriorityAffinity = GetHighestPriorityAffinity(allAffinityContexts);
        ConsumeTurnsForMultiTarget(highestPriorityAffinity, turnCtx);
        
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

        // Agrupar hits por target para mostrar correctamente
        var hitsPerTarget = new Dictionary<Unit, int>();
        for (int hit = 0; hit < numHits; hit++)
        {
            Unit target = targets[hit % targets.Count];
            if (!hitsPerTarget.ContainsKey(target))
                hitsPerTarget[target] = 0;
            hitsPerTarget[target]++;
        }

        // Procesar cada target con sus hits correspondientes
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
            
            // Mostrar HP final después de todos los hits a este target
            CombatUI.DisplayFinalHP(target);
        }

        // Mostrar HP final del atacante solo una vez si hubo repel
        if (repelTargets.Count > 0)
        {
            UnitActionManager.ApplyDamageTaken(skillCtx.Caster, totalRepelDamage);
            CombatUI.DisplayFinalHP(skillCtx.Caster);
        }

        // Consumir turnos
        string highestPriorityAffinity = GetHighestPriorityAffinity(allAffinityContexts);
        ConsumeTurnsForMultiTarget(highestPriorityAffinity, turnCtx);
        
        CombatUI.DisplaySeparator();
    }

    private static AffinityContext ProcessSingleTargetHit(SkillUseContext skillCtx, out bool isRepel, out double repelDamage)
    {
        int stat = AffinityEffectManager.GetStatForSkill(skillCtx);
        double baseDamage = AffinityEffectManager.CalculateBaseDamage(stat, skillCtx.Skill.Power);
        var affinityCtx = new AffinityContext(skillCtx.Caster, skillCtx.Target, skillCtx.Skill.Type, baseDamage);
        
        isRepel = false;
        repelDamage = 0;

        // Mostrar uso de habilidad
        DisplaySkillUsage(skillCtx.Caster, skillCtx.Skill, skillCtx.Target);

        string affinity = AffinityResolver.GetAffinity(skillCtx.Target, skillCtx.Skill.Type);
        
        if (skillCtx.Skill.Type == "Light" || skillCtx.Skill.Type == "Dark")
        {
            HandleLightDarkSkill(affinityCtx, skillCtx);
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
                repelDamage = affinityCtx.BaseDamage;
                CombatUI.DisplayRepelMessage(affinityCtx.Target, affinityCtx.Caster, (int)Math.Floor(affinityCtx.BaseDamage));
                break;
                
            default: // Neutral
                UnitActionManager.ApplyDamageTaken(affinityCtx.Target, finalDamage);
                CombatUI.DisplayDamageReceived(affinityCtx.Target, (int)Math.Floor(finalDamage));
                break;
        }
        
        CombatUI.DisplayFinalHP(affinityCtx.Target);
    }

    private static void HandleLightDarkSkill(AffinityContext affinityCtx, SkillUseContext skillCtx)
    {
        AffinityEffectManager.GetSuccessSkillsLightAndDark(affinityCtx, skillCtx);
        // Siempre mostrar HP final después de un ataque Light/Dark
        CombatUI.DisplayFinalHP(affinityCtx.Target);
    }

    private static void DisplaySkillUsage(Unit caster, Skill skill, Unit target)
    {
        CombatUI.DisplaySkillUsage(caster, skill, target);
    }

    private static string GetAttackVerbForSkillType(string skillType)
    {
        return skillType switch
        {
            "Phys" => "ataca",
            "Gun" => "dispara",
            "Light" => "ataca con luz",
            "Dark" => "ataca con oscuridad",
            "Almighty" => "lanza un ataque todo poderoso",
            _ => "lanza" // Fire, Ice, Elec, Force
        };
    }

    private static List<Unit> GetTargetsInCorrectOrder(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        List<Unit> orderedTargets = new List<Unit>();
        
        switch (skillCtx.Skill.Target)
        {
            case "All":
                // 1. Unidades en el tablero del oponente (izquierda a derecha)
                AddOpponentActiveUnits(skillCtx, orderedTargets);
                
                // 2. Unidades en la reserva del oponente (orden del archivo)
                AddOpponentReserveUnits(skillCtx, orderedTargets);
                
                // 3. Unidades en el tablero del jugador (izquierda a derecha, excluyendo caster)
                AddPlayerActiveUnitsExceptCaster(skillCtx, orderedTargets);
                
                // 4. Unidades en la reserva del jugador (orden del archivo)
                AddPlayerReserveUnits(skillCtx, orderedTargets);
                
                // 5. La unidad que utilizó la habilidad
                orderedTargets.Add(skillCtx.Caster);
                break;
                
            case "Multi":
                return GetMultiTargetsUsingAlgorithm(skillCtx, turnCtx);
        }
        
        // Filtrar solo unidades que pueden recibir el efecto
        return orderedTargets.Where(unit => ShouldReceiveEffect(unit, skillCtx.Skill)).ToList();
    }

    private static List<Unit> GetMultiTargetsUsingAlgorithm(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        PlayerUnitManager opponentUnitManager = skillCtx.Defender.UnitManager;
        var activeEnemies = opponentUnitManager.GetValidActiveUnits();
        
        if (activeEnemies.Count == 0) return new List<Unit>();

        int K = turnCtx.Attacker.TurnManager.GetConstantKPlayer();
        int A = activeEnemies.Count;
        int hits = SkillManager.CalculateNumberHits(skillCtx.Skill.Hits, turnCtx.Attacker);
        
        int i = K % A;
        bool directionLeft = (i % 2 == 0);
        
        List<Unit> selectedTargets = new List<Unit>();
        int currentIndex = i;
        
        for (int h = 0; h < hits; h++)
        {
            selectedTargets.Add(activeEnemies[currentIndex]);
            
            if (h < hits - 1) // No mover en el último hit
            {
                if (directionLeft)
                {
                    currentIndex = (currentIndex - 1 + A) % A;
                }
                else
                {
                    currentIndex = (currentIndex + 1) % A;
                }
            }
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

    private static void AddPlayerActiveUnitsExceptCaster(SkillUseContext skillCtx, List<Unit> targets)
    {
        var activeUnits = skillCtx.Attacker.UnitManager.GetActiveUnits();
        foreach (var unit in activeUnits)
        {
            if (unit != null && unit != skillCtx.Caster && unit.IsAlive())
            {
                targets.Add(unit);
            }
        }
    }

    private static void AddPlayerReserveUnits(SkillUseContext skillCtx, List<Unit> targets)
    {
        var reserveUnits = skillCtx.Attacker.UnitManager.GetReservedUnits();
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

    private static string GetHighestPriorityAffinity(List<AffinityContext> contexts)
    {
        // Prioridad: Repel/Drain > Null > Miss > Weak > Neutral/Resist
        var priorities = new Dictionary<string, int>
        {
            {"Rp", 1}, {"Dr", 1},
            {"Nu", 2},
            {"Miss", 3},
            {"Wk", 4},
            {"-", 5}, {"Rs", 5}
        };

        string highestAffinity = "-";
        int highestPriority = int.MaxValue;

        foreach (var ctx in contexts)
        {
            string affinity = AffinityResolver.GetAffinity(ctx.Target, ctx.AttackType);
            
            if (priorities.ContainsKey(affinity) && priorities[affinity] < highestPriority)
            {
                highestPriority = priorities[affinity];
                highestAffinity = affinity;
            }
        }

        return highestAffinity;
    }

    private static void ConsumeTurnsForMultiTarget(string affinity, TurnContext turnCtx)
    {
        Player attackingPlayer = turnCtx.Attacker;
        PlayerTurnManager turnManagerPlayer = attackingPlayer.TurnManager;
        
        switch (affinity)
        {
            case "Rp":
            case "Dr":
                TurnManager.ConsumeAllTurns(attackingPlayer);
                break;
            
            case "Nu":
                if (turnManagerPlayer.GetBlinkingTurns() >= 2)
                {
                    turnManagerPlayer.ConsumeBlinkingTurn(2);
                }
                else
                {
                    int blink = turnManagerPlayer.GetBlinkingTurns();
                    turnManagerPlayer.ConsumeBlinkingTurn(blink);
                    turnManagerPlayer.ConsumeFullTurn(2 - blink);
                }
                break;

            case "Miss":
                if (turnManagerPlayer.GetBlinkingTurns() >= 1)
                    turnManagerPlayer.ConsumeBlinkingTurn(1);
                else
                    turnManagerPlayer.ConsumeFullTurn(1);
                break;

            case "Wk":
                if (turnManagerPlayer.GetFullTurns() > 0)
                {
                    turnManagerPlayer.ConsumeFullTurn(1);
                    turnManagerPlayer.GainBlinkingTurn(1);
                }
                else
                {
                    turnManagerPlayer.ConsumeBlinkingTurn(1);
                }
                break;
            
            case "Rs":
            case "-":
            default:
                if (turnManagerPlayer.GetBlinkingTurns() > 0)
                {
                    turnManagerPlayer.ConsumeBlinkingTurn(1);
                }
                else
                {
                    turnManagerPlayer.ConsumeFullTurn(1);
                }
                break;
        }
    }
}