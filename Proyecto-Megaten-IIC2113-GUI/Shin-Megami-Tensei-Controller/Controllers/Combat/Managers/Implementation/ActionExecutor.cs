using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Managers.Managers.Interfaces;
using Shin_Megami_Tensei.Managers.New_Actions;

namespace Shin_Megami_Tensei.Managers.Managers.Implementation;

public class ActionExecutor : IActionExecutor
    {
        private readonly IShinMegamiTenseiView _view;
        private readonly IActionHandler _actionHandler;

        public ActionExecutor(IShinMegamiTenseiView view, IActionHandler actionHandler)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _actionHandler = actionHandler ?? throw new ArgumentNullException(nameof(actionHandler));
        }

        public bool ExecuteUnitAction(Unit activeUnit, Player currentPlayer)
        {
            if (!IsValidUnit(activeUnit))
                return false;

            var actionOptions = GetActionOptionsForUnit(activeUnit);
            _view.DisplayGameState(GetPlayersDictionary(currentPlayer), currentPlayer, actionOptions);

            return _actionHandler.HandleAction(activeUnit, currentPlayer);
        }

        private bool IsValidUnit(Unit unit)
        {
            return unit != null && IsUnitAlive(unit);
        }

        private bool IsUnitAlive(Unit unit)
        {
            return unit.GetCurrentStats().GetStatByName(StatType.HP.ToGameString()) > 0;
        }

        private List<string> GetActionOptionsForUnit(Unit unit)
        {
            return unit switch
            {
                Samurai => GetSamuraiOptions(),
                Demon => GetDemonOptions(),
                _ => new List<string>()
            };
        }

        private List<string> GetSamuraiOptions()
        {
            return new List<string>
            {
                "Atacar", "Disparar", "Usar Habilidad", "Invocar", "Pasar Turno", "Rendirse"
            };
        }

        private List<string> GetDemonOptions()
        {
            return new List<string>
            {
                "Atacar", "Usar Habilidad", "Invocar", "Pasar Turno"
            };
        }

        private Dictionary<string, Player> GetPlayersDictionary(Player currentPlayer)
        {
            // This would need to be provided by the combat manager context
            // For now, returning a simplified version
            return new Dictionary<string, Player>();
        }
    }
