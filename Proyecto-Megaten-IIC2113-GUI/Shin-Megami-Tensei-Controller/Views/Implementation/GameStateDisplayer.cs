using Shin_Megami_Tensei_View.Implementation.Interfaces;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Enums;

namespace Shin_Megami_Tensei_View.Implementation.Implementation;

public class GameStateDisplayer : IGameStateDisplayer
    {
        private readonly IDisplayService _displayService;

        public GameStateDisplayer(IDisplayService displayService)
        {
            _displayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
        }

        public void DisplayBoardState(Dictionary<string, Player> players)
        {
            int playerNumber = 1;
            foreach (var player in players.Values)
            {
                _displayService.WriteLine($"Equipo de {player.GetTeam().Samurai.GetName()} (J{playerNumber})");
                DisplayActiveUnits(player);
                playerNumber++;
            }
            _displayService.WriteLine(GameConstants.Separator);
        }

        public void DisplayTurnInfo(Player player)
        {
            var turnManager = player.TurnManager;
            _displayService.WriteLine($"Full Turns: {turnManager.GetFullTurns()}");
            _displayService.WriteLine($"Blinking Turns: {turnManager.GetBlinkingTurns()}");
            _displayService.WriteLine(GameConstants.Separator);
        }

        public void DisplaySortedUnits(Player player)
        {
            var unitManager = player.UnitManager;
            var units = unitManager.GetSortedActiveUnitsByOrderOfAttack();
            
            _displayService.WriteLine("Orden:");

            int count = 0;
            foreach (var unit in units)
            {
                if (unit is not null)
                {
                    count++;
                    _displayService.WriteLine($"{count}-{unit.GetName()}");
                }
            }
            
            _displayService.WriteLine(GameConstants.Separator);
        }

        public void DisplayRoundStart(Samurai samurai, int playerNumber)
        {
            _displayService.WriteLine($"Ronda de {samurai.GetName()} (J{playerNumber})");
            _displayService.WriteLine(GameConstants.Separator);
        }

        private void DisplayActiveUnits(Player player)
        {
            var unitManager = player.UnitManager;
            var units = unitManager.GetActiveUnits();
            char label = 'A';
        
            foreach (var unit in units)
            {
                DisplayUnitInfo(unit, player, label);
                label++;
            }
        }

        private void DisplayUnitInfo(Unit unit, Player player, char label)
        {
            if (unit == null)
            {
                DisplayEmptySlot(label);
            }
            else if (IsSamurai(unit, player))
            {
                DisplayUnitStats(label, unit);
            }
            else if (IsUnitDead(unit))
            {
                DisplayEmptySlot(label);
            }
            else
            {
                DisplayUnitStats(label, unit);
            }
        }

        private bool IsSamurai(Unit unit, Player player)
        {
            return unit == player.GetTeam().Samurai;
        }

        private bool IsUnitDead(Unit unit)
        {
            return unit.GetCurrentStats().GetStatByName(StatType.Hp.ToGameString()) <= 0;
        }

        private void DisplayEmptySlot(char label)
        {
            _displayService.WriteLine($"{label}-");
        }

        private void DisplayUnitStats(char label, Unit unit)
        {
            var currentStats = unit.GetCurrentStats();
            var baseStats = unit.GetBaseStats();
            int hp = currentStats.GetStatByName(StatType.Hp.ToGameString());
            int maxHp = baseStats.GetStatByName(StatType.Hp.ToGameString());
            int mp = currentStats.GetStatByName(StatType.Mp.ToGameString());
            int maxMp = baseStats.GetStatByName(StatType.Mp.ToGameString());
            _displayService.WriteLine($"{label}-{unit.GetName()} HP:{hp}/{maxHp} MP:{mp}/{maxMp}");
        }
    }
