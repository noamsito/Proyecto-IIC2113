using Shin_Megami_Tensei_View.Implementation.Interfaces;
using Shin_Megami_Tensei;

namespace Shin_Megami_Tensei_View.Implementation.Implementation;

public class CombatUIFacade
    {
        private readonly IGameStateDisplayer _gameStateDisplayer;
        private readonly ICombatDisplayer _combatDisplayer;
        private readonly IAffinityDisplayer _affinityDisplayer;
        private readonly IDamageDisplayer _damageDisplayer;
        private readonly IHealingDisplayer _healingDisplayer;
        private readonly ISelectionDisplayer _selectionDisplayer;
        private readonly IOptionDisplayer _optionDisplayer;
        private readonly ITurnDisplayer _turnDisplayer;
        private readonly IDisplayService _displayService;

        public CombatUIFacade(
            IGameStateDisplayer gameStateDisplayer,
            ICombatDisplayer combatDisplayer,
            IAffinityDisplayer affinityDisplayer,
            IDamageDisplayer damageDisplayer,
            IHealingDisplayer healingDisplayer,
            ISelectionDisplayer selectionDisplayer,
            IOptionDisplayer optionDisplayer,
            ITurnDisplayer turnDisplayer,
            IDisplayService displayService)
        {
            _gameStateDisplayer = gameStateDisplayer ?? throw new ArgumentNullException(nameof(gameStateDisplayer));
            _combatDisplayer = combatDisplayer ?? throw new ArgumentNullException(nameof(combatDisplayer));
            _affinityDisplayer = affinityDisplayer ?? throw new ArgumentNullException(nameof(affinityDisplayer));
            _damageDisplayer = damageDisplayer ?? throw new ArgumentNullException(nameof(damageDisplayer));
            _healingDisplayer = healingDisplayer ?? throw new ArgumentNullException(nameof(healingDisplayer));
            _selectionDisplayer = selectionDisplayer ?? throw new ArgumentNullException(nameof(selectionDisplayer));
            _optionDisplayer = optionDisplayer ?? throw new ArgumentNullException(nameof(optionDisplayer));
            _turnDisplayer = turnDisplayer ?? throw new ArgumentNullException(nameof(turnDisplayer));
            _displayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
        }
        
        public IGameStateDisplayer GameState => _gameStateDisplayer;
        public ICombatDisplayer Combat => _combatDisplayer;
        public IAffinityDisplayer Affinity => _affinityDisplayer;
        public IDamageDisplayer Damage => _damageDisplayer;
        public IHealingDisplayer Healing => _healingDisplayer;
        public ISelectionDisplayer Selection => _selectionDisplayer;
        public IOptionDisplayer Options => _optionDisplayer;
        public ITurnDisplayer Turns => _turnDisplayer;

        public string GetUserInputWithSeparator()
        {
            string input = _displayService.ReadLine();
            _turnDisplayer.DisplaySeparator();
            return input;
        }

        public void DisplayWinner(Player winner)
        {
            int playerNumber = DeterminePlayerNumber(winner);
            _displayService.WriteLine($"Ganador: {winner.GetTeam().Samurai.GetName()} (J{playerNumber})");
        }

        private int DeterminePlayerNumber(Player winner)
        {
            return winner.GetName() == PlayerConstants.PLAYER_ONE_NAME ? 1 : 2;
        }
    }
