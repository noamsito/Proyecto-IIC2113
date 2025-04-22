using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Gadgets;

public static class CombatUI
{
        private static View? _view;
        
        public static void Initialize(View view)
        {
            _view = view;
        }

        public static void DisplayBoardState(Dictionary<string, Player> players)
        {
            int playerNumber = 1;
            foreach (var player in players.Values)
            {
                _view.WriteLine($"Equipo de {player.GetTeam().Samurai.GetName()} (J{playerNumber})");
                DisplayActiveUnits(player);
                playerNumber++;
            }
            _view.WriteLine(GameConstants.Separator);
        }
    
        public static void DisplayActiveUnits(Player player)
        {
            var units = player.GetActiveUnits();
            char label = 'A';
    
            foreach (var unit in units)
            {
                if (unit == null)
                {
                    _view.WriteLine($"{label}-");
                }
                else
                {
                    _view.WriteLine($"{label}-{unit.GetName()} HP:{unit.GetCurrentStats().GetStatByName("HP")}/{unit.GetBaseStats().GetStatByName("HP")} MP:{unit.GetCurrentStats().GetStatByName("MP")}/{unit.GetBaseStats().GetStatByName("MP")}");
                }
                label++;
            }
        }
    
        public static void DisplayTurnInfo(Player player)
        {
            _view.WriteLine($"Full Turns: {player.GetFullTurns()}");
            _view.WriteLine($"Blinking Turns: {player.GetBlinkingTurns()}");
            _view.WriteLine(GameConstants.Separator);
        }
    
        public static void DisplaySortedUnits(Player player)
        {
            _view.WriteLine("Orden:");
            var units = player.GetSortedActiveUnitsByOrderOfAttack();
            for (int i = 0; i < units.Count; i++)
                _view.WriteLine($"{i + 1}-{units[i].GetName()}");
            _view.WriteLine(GameConstants.Separator);
        }
    
        public static void DisplayWinner(Player winner)
        {
            int playerNumber = winner.GetName() == "Player 1" ? 1 : 2;
            _view.WriteLine($"Ganador: {winner.GetTeam().Samurai.GetName()} (J{playerNumber})");
        }
    
        public static void DisplayAttack(string attackerName, string targetName, string attackType)
        {
            string action = attackType switch
            {
                "Phys" => "ataca a",
                "Gun" => "dispara a",
                "Fire" => "lanza fuego a",
                "Ice" => "lanza hielo a",
                "Elec" => "lanza electricidad a",
                "Force" => "lanza viento a",
                _ => "ataca a"
            };
    
            _view.WriteLine($"{attackerName} {action} {targetName}");
        }
    
        public static void DisplayDamageResult(Unit target, int damage)
        {
            _view.WriteLine($"{target.GetName()} recibe {damage} de daño");
            _view.WriteLine($"{target.GetName()} termina con HP:{target.GetCurrentStats().GetStatByName("HP")}/{target.GetBaseStats().GetStatByName("HP")}");
            _view.WriteLine(GameConstants.Separator);
        }
    
        public static void DisplayHealing(Unit target, int amount)
        {
            _view.WriteLine($"{target.GetName()} recibe {amount} de HP");
            _view.WriteLine($"{target.GetName()} termina con HP:{target.GetCurrentStats().GetStatByName("HP")}/{target.GetBaseStats().GetStatByName("HP")}");
            _view.WriteLine(GameConstants.Separator);
        }
    
        public static void DisplayAffinityMessage(string affinityType, string attackerName, string targetName)
        {
            string msg = affinityType switch
            {
                "Wk" => $"{targetName} es débil contra el ataque de {attackerName}",
                "Rs" => $"{targetName} es resistente al ataque de {attackerName}",
                "Nu" => $"{targetName} bloquea el ataque de {attackerName}",
                "Rp" => $"{targetName} devuelve el ataque de {attackerName}",
                "Dr" => $"{targetName} absorbe el daño de {attackerName}"
            };
    
            if (!string.IsNullOrEmpty(msg))
                _view.WriteLine(msg);
        }
    
        public static void DisplayTurnChanges(int fullConsumed, int blinkingConsumed, int blinkingGained)
        {
            _view.WriteLine($"Se han consumido {fullConsumed} Full Turn(s) y {blinkingConsumed} Blinking Turn(s)");
            _view.WriteLine($"Se han obtenido {blinkingGained} Blinking Turn(s)");
            _view.WriteLine(GameConstants.Separator);
        }
    
        public static void DisplaySkillUsage(Unit caster, Skill skill, Unit target)
        {
            string action = skill.Type switch
            {
                "Fire" => "lanza fuego a",
                "Ice" => "lanza hielo a",
                "Elec" => "lanza electricidad a",
                "Force" => "lanza viento a",
                _ => "usa " + skill.Name + " en"
            };
    
            _view.WriteLine($"{caster.GetName()} {action} {target.GetName()}");
        }
    
        public static void DisplayRevive(Unit caster, Unit revived, int healAmount)
        {
            _view.WriteLine($"{caster.GetName()} revive a {revived.GetName()}");
            _view.WriteLine($"{revived.GetName()} recibe {healAmount} de HP");
            _view.WriteLine($"{revived.GetName()} termina con HP:{revived.GetCurrentStats().GetStatByName("HP")}/{revived.GetBaseStats().GetStatByName("HP")}");
            _view.WriteLine(GameConstants.Separator);
        }
    
        public static void DisplaySummon(Unit summoned)
        {
            _view.WriteLine($"{summoned.GetName()} ha sido invocado");
            _view.WriteLine(GameConstants.Separator);
        }
        
        public static void DisplaySummonOptions(List<Unit> reserve, View view)
        {
            for (int i = 0; i < reserve.Count; i++)
            {
                var demon = reserve[i];
                view.WriteLine($"{i + 1}-{demon.GetName()} " +
                               $"HP:{demon.GetCurrentStats().GetStatByName("HP")}/{demon.GetBaseStats().GetStatByName("HP")} " +
                               $"MP:{demon.GetCurrentStats().GetStatByName("MP")}/{demon.GetBaseStats().GetStatByName("MP")}");
            }        
            view.WriteLine($"{reserve.Count + 1}-Cancelar");
        }

        public static void DisplayHasBeenSummoned(Unit newDemonAdded)
        {
            _view.WriteLine(GameConstants.Separator);
            _view.WriteLine($"{newDemonAdded.GetName()} ha sido invocado");
            _view.WriteLine(GameConstants.Separator);
        }
    }