using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Views.Interfaces;

public class TargetSelectionService : ITargetSelectionService
    {
        private readonly SMTGUI _gui;
        private Dictionary<string, Player> _players;

        public TargetSelectionService(SMTGUI gui)
        {
            _gui = gui ?? throw new ArgumentNullException(nameof(gui));
        }

        public Unit SelectTargetUnit(TargetSelectionContext context)
        {
            var validTargets = GetValidAttackTargets(context.TargetPlayer);
            
            if (!validTargets.Any())
                return null;

            return ProcessTargetSelection(context, validTargets);
        }

        public Skill SelectSkillFromUnit(Unit unit)
        {
            var availableSkills = GetAffordableSkills(unit);
            
            if (!availableSkills.Any())
            {
                return null;
            }
            
            var skillOptions = CreateSkillOptions(availableSkills);
            ShowSelectionInterface(skillOptions);

            var clickedElement = WaitForValidSelection();
            
            if (IsSelectionCancelled(clickedElement))
                return null;

            return FindSelectedSkill(clickedElement.Text, availableSkills);
        }

        private Unit GetUnitAtPosition(Player player, int slotIndex)
        {
            
        }

        public Unit SelectSummonableUnit(Player player)
        {
            var reserveUnits = player.UnitManager.GetReservedUnits()
                .Where(u => u != null && IsUnitAlive(u))
                .ToList();
            
            if (!reserveUnits.Any())
            {
                return null;
            }
            
            var unitOptions = CreateSummonOptions(reserveUnits);
            ShowSelectionInterface(unitOptions);

            var clickedElement = WaitForValidSelection();

            if (IsSelectionCancelled(clickedElement))
                return null;

            if (clickedElement.Type == ClickedElementType.UnitInReserve)
            {
                return FindUnitByName(player, clickedElement.Text);
            }

            return null;
        }

        public int SelectBoardSlot(Player player)
        {
            var slotOptions = new List<string> { "Haz click en el slot destino", "Cancelar" };
            ShowSelectionInterface(slotOptions);

            var clickedElement = WaitForValidSelection();

            if (IsSelectionCancelled(clickedElement))
                return -1;

            return FindSlotIndex(player, clickedElement);
        }

        private List<Unit> GetValidAttackTargets(Player targetPlayer)
        {
            return targetPlayer.UnitManager.GetActiveUnits()
                .Where(unit => unit != null && IsUnitAlive(unit))
                .ToList();
        }

        private Unit ProcessTargetSelection(TargetSelectionContext context, List<Unit> validTargets)
        {
            var targetOptions = CreateTargetOptions(validTargets);
            ShowSelectionInterface(targetOptions);

            while (true)
            {
                var clickedElement = _gui.GetClickedElement();

                if (IsSelectionCancelled(clickedElement))
                    return null;

                var selectedTarget = ProcessTargetClick(clickedElement, context.TargetPlayer);
                if (selectedTarget != null)
                    return selectedTarget;
            }
        }

        private List<Skill> GetAffordableSkills(Unit unit)
        {
            var currentMana = unit.GetCurrentStats().GetStatByName("MP");
            return unit.GetSkills().Where(skill => skill.Cost <= currentMana).ToList();
        }

        private List<string> CreateSkillOptions(List<Skill> skills)
        {
            var options = skills.Select(skill => $"{skill.Name} - MP: {skill.Cost}").ToList();
            options.Add("Cancelar");
            return options;
        }

        private List<string> CreateTargetOptions(List<Unit> targets)
        {
            var options = targets.Select(unit => $"Atacar a {unit.GetName()}").ToList();
            options.Add("Cancelar");
            return options;
        }

        private List<string> CreateSummonOptions(List<Unit> units)
        {
            var options = units.Select(unit => unit.GetName()).ToList();
            options.Add("Cancelar");
            return options;
        }

        private void ShowSelectionInterface(List<string> options)
        {
            if (_players == null || !_players.Any())
            {
                throw new InvalidOperationException("Players must be set before showing selection interface");
            }
            
            var dummyPlayer = _players.Values.First();
            var gameState = new GameStateAdapter(_players, dummyPlayer, options);
            _gui.Update(gameState);
        }

        private IClickedElement WaitForValidSelection()
        {
            IClickedElement clickedElement;
            do
            {
                clickedElement = _gui.GetClickedElement();
            } while (clickedElement.Type != ClickedElementType.Button && 
                     clickedElement.Type != ClickedElementType.UnitInBoard &&
                     clickedElement.Type != ClickedElementType.UnitInReserve);

            return clickedElement;
        }

        private bool IsSelectionCancelled(IClickedElement element)
        {
            return element.Type == ClickedElementType.Button && element.Text == "Cancelar";
        }

        private Skill FindSelectedSkill(string buttonText, List<Skill> availableSkills)
        {
            var skillName = buttonText.Split(" - ")[0];
            return availableSkills.FirstOrDefault(skill => skill.Name == skillName);
        }

        private Unit ProcessTargetClick(IClickedElement clickedElement, Player targetPlayer)
        {
            if (clickedElement.Type == ClickedElementType.Button && clickedElement.Text.StartsWith("Atacar a "))
            {
                var targetName = clickedElement.Text.Substring("Atacar a ".Length);
                return FindUnitByName(targetPlayer, targetName);
            }

            if (clickedElement.Type == ClickedElementType.UnitInBoard && 
                IsClickOnTargetPlayer(clickedElement, targetPlayer))
            {
                return FindUnitByName(targetPlayer, clickedElement.Text);
            }

            return null;
        }

        private bool IsClickOnTargetPlayer(IClickedElement clickedElement, Player targetPlayer)
        {
            return clickedElement.PlayerId != null && 
                   GetPlayerById(clickedElement.PlayerId) == targetPlayer;
        }

        private int FindSlotIndex(Player player, IClickedElement clickedElement)
        {
            if (clickedElement.Type != ClickedElementType.UnitInBoard)
                return -1;

            var activeUnits = player.UnitManager.GetActiveUnits();
            
            for (int i = 0; i < activeUnits.Count; i++)
            {
                var isEmptySlot = activeUnits[i] == null && clickedElement.Text == "-";
                var isSameUnit = activeUnits[i]?.GetName() == clickedElement.Text;
                
                if (isEmptySlot || isSameUnit)
                    return i;
            }

            return -1;
        }

        private Unit FindUnitByName(Player player, string unitName)
        {
            var foundUnit = FindInActiveUnits(player, unitName) ?? FindInReserveUnits(player, unitName);
            return foundUnit;
        }

        private Unit FindInActiveUnits(Player player, string unitName)
        {
            return player.UnitManager.GetActiveUnits()
                .FirstOrDefault(unit => unit != null && 
                               unit.GetName().Equals(unitName, StringComparison.OrdinalIgnoreCase));
        }

        private Unit FindInReserveUnits(Player player, string unitName)
        {
            return player.UnitManager.GetReservedUnits()
                .FirstOrDefault(unit => unit != null && 
                               unit.GetName().Equals(unitName, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsUnitAlive(Unit unit)
        {
            return unit.GetCurrentStats().GetStatByName("HP") > 0;
        }

        private Player GetPlayerById(int? playerId)
        {
            if (_players == null)
                throw new InvalidOperationException("Players not initialized");
            
            if (playerId == null)
                throw new ArgumentException("Player ID cannot be null");

            return playerId == 1 ? _players["Player 1"] : _players["Player 2"];
        }

        public void SetPlayers(Dictionary<string, Player> players)
        {
            _players = players ?? throw new ArgumentNullException(nameof(players));
        }
    }
