using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei.Models.Constants;

public class GUICombatManager
{
    private readonly IShinMegamiTenseiView _view;
    private readonly Dictionary<string, Player> _players;
    private bool _gameWon;
    private bool _isNewRound;

    private const string Player1Key = "Player 1";
    private const string Player2Key = "Player 2";

    public GUICombatManager(IShinMegamiTenseiView view, Dictionary<string, Player> players)
    {
        _view = view;
        _players = players;
        _gameWon = false;
        _isNewRound = true;
    }



    public void StartCombat()
    {
        InitializePlayersForCombat();
        ExecuteCombatLoop();
    }

    private void InitializePlayersForCombat()
    {
        foreach (var player in _players.Values)
        {
            PlayerTurnManager turnManager = player.TurnManager;
            turnManager.SetTurns();
        }
    }

    private void ExecuteCombatLoop()
    {
        Player currentPlayer = _players[Player1Key];

        while (!_gameWon)
        {
            ProcessPlayerTurn(currentPlayer);

            if (ShouldSwitchPlayer(currentPlayer))
            {
                currentPlayer = GetOpponent(currentPlayer);
                _isNewRound = true;
            }
        }
    }

    private void ProcessPlayerTurn(Player currentPlayer)
    {
        int playerNumber = GetPlayerNumber(currentPlayer);

        HandleNewRoundIfNeeded(currentPlayer, playerNumber);
        
        Unit activeUnit = TurnManager.GetCurrentUnit(currentPlayer);
        if (activeUnit == null || !IsUnitAlive(activeUnit))
        {
            ConsumeCurrentTurn(currentPlayer);
            return;
        }
        
        var currentOptions = GetCurrentPlayerOptions(currentPlayer);
        _view.DisplayGameState(_players, currentPlayer, currentOptions);

        bool actionWasExecuted = ExecuteUnitAction(currentPlayer);
        
        if (!actionWasExecuted)
        {
            ConsumeCurrentTurn(currentPlayer);
        }
        
        CheckForVictory(currentPlayer);
    }

    private List<string> GetCurrentPlayerOptions(Player currentPlayer)
    {
        Unit activeUnit = TurnManager.GetCurrentUnit(currentPlayer);
        
        if (activeUnit == null)
            return new List<string>();

        return activeUnit switch
        {
            Samurai => new List<string>
            {
                "Atacar",
                "Disparar", 
                "Usar Habilidad", 
                "Invocar", 
                "Pasar Turno", 
                "Rendirse"
            },
            Demon => new List<string>
            {
                "Atacar", 
                "Usar Habilidad", 
                "Invocar",
                "Pasar Turno"
            },
            _ => new List<string>()
        };
    }

    private bool ExecuteUnitAction(Player currentPlayer)
    {
        Unit? activeUnit = TurnManager.GetCurrentUnit(currentPlayer);
        
        if (activeUnit == null || !IsUnitAlive(activeUnit))
        {
            return false;
        }

        return TryExecuteActionWithGui(activeUnit, currentPlayer);
    }

    private bool TryExecuteActionWithGui(Unit unit, Player currentPlayer)
    {
        string playerChoice = _view.GetPlayerChoice();
        Player opponent = GetOpponent(currentPlayer);
        
        return unit switch
        {
            Samurai samurai => TryExecuteSamuraiAction(samurai, playerChoice, currentPlayer, opponent),
            Demon demon => TryExecuteDemonAction(demon, playerChoice, currentPlayer, opponent),
            _ => false
        };
    }

    private bool TryExecuteSamuraiAction(Samurai samurai, string choice, Player currentPlayer, Player opponent)
    {
        return choice switch
        {
            "1" => TryExecuteBasicAttackAction(samurai, AttackType.Phys, currentPlayer, opponent),
            "2" => TryExecuteBasicAttackAction(samurai, AttackType.Gun, currentPlayer, opponent), 
            "3" => TryExecuteSkillAction(samurai, currentPlayer, opponent),
            "4" => TryExecuteSummonAction(currentPlayer, samurai),
            "5" => TryExecutePassTurnAction(currentPlayer),
            "6" => TryExecuteSurrenderAction(currentPlayer),
            _ => false
        };
    }

    private bool TryExecuteDemonAction(Demon demon, string choice, Player currentPlayer, Player opponent)
    {
        return choice switch
        {
            "1" => TryExecuteBasicAttackAction(demon, AttackType.Phys, currentPlayer, opponent),
            "2" => TryExecuteSkillAction(demon, currentPlayer, opponent),
            "3" => TryExecuteSummonAction(currentPlayer, demon),
            "4" => TryExecutePassTurnAction(currentPlayer),
            _ => false
        };
    }

    private bool TryExecuteBasicAttackAction(Unit attacker, AttackType attackType, Player currentPlayer, Player opponent)
    {
        Unit target = _view.SelectTarget(opponent);
        if (target == null) return false;

        try
        {
            double baseDamage = CalculateBaseDamageByType(attacker, attackType);
            var affinityContext = new AffinityContext(attacker, target, attackType, baseDamage);
            
            ApplyAttackEffectsManually(affinityContext);
            
            var turnContext = CreateTurnContext(currentPlayer, opponent);
            ConsumeTurnsBasedOnAffinitySimplified(affinityContext, turnContext);
            UpdateGameStateAfterAction(turnContext);
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void ApplyAttackEffectsManually(AffinityContext affinityCtx)
    {
        double finalDamage = AffinityEffectManager.GetDamageBasedOnAffinity(affinityCtx);
        
        if (finalDamage > 0)
        {
            UnitActionManager.ApplyDamageTaken(affinityCtx.Target, finalDamage);
        }
        else if (finalDamage == -1)
        {
            UnitActionManager.ApplyHealToUnit(affinityCtx.Target, affinityCtx.BaseDamage);
        }
        else if (finalDamage == -2)
        {
            UnitActionManager.ApplyDamageTaken(affinityCtx.Caster, affinityCtx.BaseDamage);
        }
    }

    private void ConsumeTurnsBasedOnAffinitySimplified(AffinityContext affinityCtx, TurnContext turnCtx)
    {
        Player attackingPlayer = turnCtx.Attacker;
        PlayerTurnManager turnManager = attackingPlayer.TurnManager;
        
        string affinity = AffinityResolver.GetAffinity(affinityCtx.Target, affinityCtx.AttackType);

        switch (affinity)
        {
            case "Rp":
            case "Dr":
                turnManager.ConsumeFullTurn(turnManager.GetFullTurns());
                turnManager.ConsumeBlinkingTurn(turnManager.GetBlinkingTurns());
                break;
            
            case "Nu":
                if (turnManager.GetBlinkingTurns() >= 2)
                {
                    turnManager.ConsumeBlinkingTurn(2);
                }
                else
                {
                    int blink = turnManager.GetBlinkingTurns();
                    turnManager.ConsumeBlinkingTurn(blink);
                    turnManager.ConsumeFullTurn(2 - blink);
                }
                break;

            case "Wk":
                if (turnManager.GetFullTurns() > 0)
                {
                    turnManager.ConsumeFullTurn(1);
                    turnManager.GainBlinkingTurn(1);
                }
                else
                {
                    turnManager.ConsumeBlinkingTurn(1);
                }
                break;
            
            default:
                if (turnManager.GetBlinkingTurns() > 0)
                {
                    turnManager.ConsumeBlinkingTurn(1);
                }
                else
                {
                    turnManager.ConsumeFullTurn(1);
                }
                break;
        }
    }

    private bool TryExecuteSkillAction(Unit caster, Player currentPlayer, Player opponent)
    {
        try
        {
            Skill skill = _view.SelectSkill(caster);
            if (skill == null) return false;

            if (skill.Cost > caster.GetCurrentStats().GetStatByName("MP"))
                return false;

            bool success = HandleSkillSimplified(caster, skill, currentPlayer, opponent);

            if (success)
            {
                int currentMP = caster.GetCurrentStats().GetStatByName("MP");
                caster.GetCurrentStats().SetStatByName("MP", Math.Max(0, currentMP - skill.Cost));
                
                currentPlayer.TurnManager.IncreaseConstantKPlayer();
                
                var turnContext = CreateTurnContext(currentPlayer, opponent);
                UpdateGameStateAfterAction(turnContext);
            }

            return success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ExecuteSkillAction: {ex.Message}");
            return false;
        }
    }

    private bool HandleSkillSimplified(Unit caster, Skill skill, Player currentPlayer, Player opponent)
    {
        try
        {
            var turnContext = CreateTurnContext(currentPlayer, opponent);
            
            Unit target = null;
         if (skill.Target == SkillTarget.Single)
            {
                target = _view.SelectTarget(opponent);
                if (target == null) return false;
            }
            else if (skill.Target == SkillTarget.Ally)
            {
                target = _view.SelectTarget(currentPlayer);
                if (target == null) return false;
            }
            
            switch (skill.Type)
            {
                case AttackType.Heal:
                    if (target != null)
                    {
                        int baseHp = target.GetBaseStats().GetStatByName(CombatConstants.HP_STAT);
                        double healAmount = Math.Floor((skill.Power / 100.0) * baseHp);
                        UnitActionManager.ApplyHealToUnit(target, healAmount);
                    }
                    break;
            
                case AttackType.Phys:
                case AttackType.Gun:
                case AttackType.Fire:
                case AttackType.Ice:
                case AttackType.Elec:
                case AttackType.Force:
                case AttackType.Almighty:
                    if (target != null)
                    {
                        int stat = GetStatForSkillType(caster, skill.Type.ToString());
                        double baseDamage = Math.Sqrt(stat * skill.Power);
                        var affinityCtx = new AffinityContext(caster, target, skill.Type, baseDamage);
                        ApplyAttackEffectsManually(affinityCtx);
                    }
                    break;
            }
            TurnManager.ConsumeTurn(turnContext);
            UpdateGameStateAfterAction(turnContext);   
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in HandleSkillSimplified: {ex.Message}");
            return false;
        }
    }
    
    private int GetStatForSkillType(Unit caster, string skillType)
    {
        return skillType switch
        {
            CombatConstants.PHYS_TYPE => caster.GetCurrentStats().GetStatByName("Str"),
            CombatConstants.GUN_TYPE => caster.GetCurrentStats().GetStatByName("Skl"),
            "Fire" or "Ice" or "Elec" or CombatConstants.FORCE_TYPE or "Almighty" => caster.GetCurrentStats().GetStatByName("Mag"),
            _ => caster.GetCurrentStats().GetStatByName("Mag")
        };
    }

    private bool TryExecuteSummonAction(Player currentPlayer, Unit currentUnit)
    {
        try
        {
            if (!ValidateSummonConditions(currentPlayer))
                return false;
    
            Unit summonTarget = SelectSummonTarget(currentPlayer);
            if (summonTarget == null)
                return false;
    
            int slot = _view.SelectSlot(currentPlayer);
            if (slot < 1)
                return false;
    
            if (!PerformSummon(currentPlayer, summonTarget, slot))
                return false;
    
            UpdateBoardAfterSummon(currentPlayer, summonTarget, slot);
    
            var turnContext = CreateTurnContext(currentPlayer, GetOpponent(currentPlayer));
            TurnManager.ConsumeTurn(turnContext);
            UpdateGameStateAfterAction(turnContext);
    
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ExecuteSummonAction: {ex.Message}");
            return false;
        }
    }
    
    private bool ValidateSummonConditions(Player currentPlayer)
    {
        var reserveUnits = currentPlayer.UnitManager.GetReservedUnits();
        return reserveUnits != null && reserveUnits.Count > 0;
    }
    
    private Unit SelectSummonTarget(Player currentPlayer)
    {
        return _view.SelectSummonTarget(currentPlayer);
    }
    
    private bool PerformSummon(Player currentPlayer, Unit summonTarget, int slot)
    {
        var activeUnits = currentPlayer.UnitManager.GetActiveUnits();
        var reserveUnits = currentPlayer.UnitManager.GetReservedUnits();
    
        if (slot >= activeUnits.Count)
            return false;
    
        Unit replacedUnit = activeUnits[slot];
        activeUnits[slot] = summonTarget;
    
        if (replacedUnit != null && replacedUnit.GetCurrentStats().GetStatByName("HP") > 0)
        {
            reserveUnits.Add(replacedUnit);
        }
    
        reserveUnits.Remove(summonTarget);
        return true;
    }
    
    private void UpdateBoardAfterSummon(Player currentPlayer, Unit summonTarget, int slot)
    {
        var activeUnits = currentPlayer.UnitManager.GetActiveUnits();
        Unit replacedUnit = activeUnits[slot];
    
        var sortedUnits = currentPlayer.UnitManager.GetSortedActiveUnitsByOrderOfAttack();
        if (replacedUnit != null)
        {
            for (int i = 0; i < sortedUnits.Count; i++)
            {
                if (sortedUnits[i] == replacedUnit)
                {
                    sortedUnits[i] = summonTarget;
                    break;
                }
            }
        }
        else
        {
            sortedUnits.Add(summonTarget);
        }
    }

    private bool TryExecutePassTurnAction(Player currentPlayer)
    {
        try
        {
            var turnContext = CreateTurnContext(
                currentPlayer, GetOpponent(currentPlayer));

            var turnManager = currentPlayer.TurnManager;
            if (turnManager.GetBlinkingTurns() > 0)
                turnManager.ConsumeBlinkingTurn(1);
            else
            {
                turnManager.ConsumeFullTurn(1);
                turnManager.GainBlinkingTurn(1);
            }

            currentPlayer.UnitManager.RearrangeSortedUnitsWhenAttacked();
            UpdateGameStateAfterAction(turnContext);
            return true;
        }
        catch
        {
            return false;
        }
    }


    private bool TryExecuteSurrenderAction(Player currentPlayer)
    {
        currentPlayer.Surrender();
        return true;
    }

    private double CalculateBaseDamageByType(Unit attacker, AttackType attackType)
    {
        return attackType == AttackType.Phys
            ? AttackExecutor.ExecutePhysicalAttack(attacker, GameConstants.MODIFIER_PHYS_DAMAGE)
            : AttackExecutor.ExecuteGunAttack(attacker, GameConstants.MODIFIER_GUN_DAMAGE);
    }

    private TurnContext CreateTurnContext(Player currentPlayer, Player opponent)
    {
        PlayerTurnManager turnManager = currentPlayer.TurnManager;
        
        int fullStart = turnManager.GetFullTurns();
        int blinkStart = turnManager.GetBlinkingTurns();

        return new TurnContext(currentPlayer, opponent, fullStart, blinkStart);
    }

    private void UpdateGameStateAfterAction(TurnContext turnContext)
    {
        UpdateDeadUnitsManually(turnContext.Defender);
        
        turnContext.Attacker.UnitManager.RearrangeSortedUnitsWhenAttacked();
        
        var currentOptions = GetCurrentPlayerOptions(turnContext.Attacker);
        _view.DisplayGameState(_players, turnContext.Attacker, currentOptions);
    }

    private void UpdateDeadUnitsManually(Player player)
    {
        if (player == null) return;
    
        var unitManager = player.UnitManager;
        var activeUnits = unitManager.GetActiveUnits();
    
        var deadUnits = CollectDeadUnits(activeUnits);
    
        foreach (var (unit, index) in deadUnits)
        {
            ProcessDeadUnit(unitManager, unit, index);
        }
    
        player.CombatState.UpdateTeamContinuationStatus();
    }
    
    private List<(Unit unit, int index)> CollectDeadUnits(List<Unit> activeUnits)
    {
        var deadUnits = new List<(Unit, int)>();
        for (int i = 0; i < activeUnits.Count; i++)
        {
            var unit = activeUnits[i];
            if (unit != null && unit.GetCurrentStats().GetStatByName("HP") <= 0 && !(unit is Samurai))
            {
                deadUnits.Add((unit, i));
            }
        }
        return deadUnits;
    }
    
    private void ProcessDeadUnit(PlayerUnitManager unitManager, Unit unit, int index)
    {
        unitManager.GetReservedUnits().Add(unit);
        unitManager.GetActiveUnits()[index] = null;
        UpdateBoardSlots(unitManager, unit);
    }
    
    private void UpdateBoardSlots(PlayerUnitManager unitManager, Unit unit)
    {
        var sortedUnits = unitManager.GetSortedActiveUnitsByOrderOfAttack();
        for (int j = 0; j < sortedUnits.Count; j++)
        {
            if (sortedUnits[j] == unit)
            {
                sortedUnits.RemoveAt(j);
                break;
            }
        }
    }

    private void HandleNewRoundIfNeeded(Player currentPlayer, int playerNumber)
    {
        if (_isNewRound)
        {
            PrepareNewRoundForGUI(currentPlayer, playerNumber);
            _isNewRound = false;
        }
    }

    private void PrepareNewRoundForGUI(Player player, int playerNumber)
    {
        PlayerTurnManager turnManagerPlayer = player.TurnManager;
        PlayerUnitManager unitManagerPlayer = player.UnitManager;
        
        turnManagerPlayer.SetTurns();
        
        unitManagerPlayer.SetOrderOfAttackOfActiveUnits();
    }

    private void ConsumeCurrentTurn(Player currentPlayer)
    {
        PlayerTurnManager turnManager = currentPlayer.TurnManager;
        
        if (turnManager.GetBlinkingTurns() > 0)
        {
            turnManager.ConsumeBlinkingTurn(1);
        }
        else if (turnManager.GetFullTurns() > 0)
        {
            turnManager.ConsumeFullTurn(1);
        }
        
        currentPlayer.UnitManager.RearrangeSortedUnitsWhenAttacked();
    }

    private bool IsUnitAlive(Unit unit)
    {
        return unit.GetCurrentStats().GetStatByName("HP") > 0;
    }

    private int GetPlayerNumber(Player player)
    {
        return player.GetName() == Player1Key ? 1 : 2;
    }

    private bool ShouldSwitchPlayer(Player currentPlayer)
    {
        return currentPlayer.TurnManager.IsPlayerOutOfTurns();
    }

    public Player GetOpponent(Player currentPlayer)
    {
        return currentPlayer.GetName() == Player1Key ? _players[Player2Key] : _players[Player1Key];
    }

    private void CheckForVictory(Player currentPlayer)
    {
        Player opponent = GetOpponent(currentPlayer);

        UpdateTeamsStatus(currentPlayer, opponent);

        if (!currentPlayer.CombatState.IsTeamAbleToContinue())
        {
            AnnounceWinner(opponent);
            return;
        }

        if (!opponent.CombatState.IsTeamAbleToContinue())
        {
            AnnounceWinner(currentPlayer);
        }
    }

    private void UpdateTeamsStatus(Player currentPlayer, Player opponent)
    {
        currentPlayer.CombatState.CheckIfTeamIsAbleToContinue();
        opponent.CombatState.CheckIfTeamIsAbleToContinue();
    }

    private void AnnounceWinner(Player winner)
    {
        _view.ShowWinner(winner);
        _gameWon = true;
    }
}
