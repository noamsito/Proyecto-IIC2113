using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;

public static class CombatUI
{
        private static View _view;
        
        public static void Initialize(View view)
        {
            _view = view;
        }

        public static string GetUserInput()
        {
            return _view.ReadLine();
        }
        
        public static void DisplaySeparator()
        {
            _view.WriteLine(GameConstants.Separator);
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
                else if (unit == player.GetTeam().Samurai)
                {
                    _view.WriteLine($"{label}-{unit.GetName()} HP:{unit.GetCurrentStats().GetStatByName("HP")}/{unit.GetBaseStats().GetStatByName("HP")} MP:{unit.GetCurrentStats().GetStatByName("MP")}/{unit.GetBaseStats().GetStatByName("MP")}");
                }
                else if (unit.GetCurrentStats().GetStatByName("HP") <= 0)
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
        
        public static void DisplayDemonsStats(List<Unit> targets)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                Unit unit = targets[i];
                string statusInfo = TargetSelector.FormatUnitStatus(unit);
                _view.WriteLine($"{i + 1}-{unit.GetName()} {statusInfo}");
            }
        }
        
        public static void DisplayCancelOption(int optionsCount)
        {
            _view.WriteLine($"{optionsCount + 1}-Cancelar");
        }
        
        public static void DisplaySkillSelectionPrompt(string unitName)
        {
            _view.WriteLine($"Seleccione una habilidad para que {unitName} use");
        }
        
        public static void DisplaySummonPrompt()
        {
            _view.WriteLine("Seleccione un monstruo para invocar");
        }
        
        public static void DisplaySlotSelectionPrompt()
        {
            _view.WriteLine("Seleccione una posición para invocar");
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
    
        private static void DisplayFullDamageResult(Unit target, double damage)
        {
            DisplayDamageTaken(target, damage);
            DisplayFinalHP(target);
            DisplaySeparator();
        }

        private static void DisplayDamageTaken(Unit target, double damage)
        {
            _view.WriteLine($"{target.GetName()} recibe {Convert.ToInt32(Math.Floor(damage))} de daño");
        }

        private static void DisplayFinalHP(Unit target)
        {
            int currentHP = target.GetCurrentStats().GetStatByName("HP");
            int baseHP = target.GetBaseStats().GetStatByName("HP");
            _view.WriteLine($"{target.GetName()} termina con HP:{currentHP}/{baseHP}");
        }
    
        public static void DisplayHealing(Unit target, double amountDamage)
        {
            int currentHp = target.GetCurrentStats().GetStatByName("HP");
            int baseHp = target.GetBaseStats().GetStatByName("HP");
            int amountHealed = Convert.ToInt32(Math.Floor(amountDamage));
            
            _view.WriteLine($"{target.GetName()} recibe {amountHealed} de HP");
            _view.WriteLine($"{target.GetName()} termina con HP:{currentHp}/{baseHp}");
            _view.WriteLine(GameConstants.Separator);
        }
    
        public static void DisplayAffinityMessage(AffinityContext affinityCtx)
        {
            string affinityType = AffinityResolver.GetAffinity(affinityCtx.Target, affinityCtx.AttackType);
            string targetName = affinityCtx.Target.GetName();
            string attackerName = affinityCtx.Caster.GetName();
            int damage = Convert.ToInt32(Math.Floor(affinityCtx.BaseDamage));
            
            string msg = affinityType switch
            {
                "Wk" => $"{targetName} es débil contra el ataque de {attackerName}",
                "Rs" => $"{targetName} es resistente el ataque de {attackerName}",
                "Nu" => $"{targetName} bloquea el ataque de {attackerName}",
                "Rp" => $"{targetName} devuelve {damage} daño a {attackerName}",
                "Dr" => $"{targetName} absorbe {damage} daño",
                "-" => ""
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
                "Phys" => "ataca a",
                "Gun" => "dispara a",
                "Heal" when skill.Name is "Recarm" or "Samarecarm" or "Invitation" => "revive a",
                "Heal" => "cura a",
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
        
        public static void DisplaySummonOptions(List<Unit> reserve)
        {
            int reservedAlive = 1;
            for (int i = 0; i < reserve.Count; i++)
            {
                var demon = reserve[i];
                if (demon.GetCurrentStats().GetStatByName("HP") > 0)
                {
                    _view.WriteLine($"{reservedAlive}-{demon.GetName()} " +
                                   $"HP:{demon.GetCurrentStats().GetStatByName("HP")}/{demon.GetBaseStats().GetStatByName("HP")} " +
                                   $"MP:{demon.GetCurrentStats().GetStatByName("MP")}/{demon.GetBaseStats().GetStatByName("MP")}");
                    reservedAlive++;
                }
            }
            
            _view.WriteLine($"{reservedAlive}-Cancelar");
        }

        public static void DisplayUnitHasBeenRevived(Unit unitCaster, Unit unitRevived)
        {
            _view.WriteLine($"{unitCaster} revive a {unitRevived}");
        }
        
        public static void DisplaySummonOptionsIncludingDead(List<Unit> reserve)
        {
            int reservedAlive = 1;
            for (int i = 0; i < reserve.Count; i++)
            {
                var demon = reserve[i];
                _view.WriteLine($"{reservedAlive}-{demon.GetName()} " +
                               $"HP:{demon.GetCurrentStats().GetStatByName("HP")}/{demon.GetBaseStats().GetStatByName("HP")} " +
                               $"MP:{demon.GetCurrentStats().GetStatByName("MP")}/{demon.GetBaseStats().GetStatByName("MP")}");
                reservedAlive++;
            
            }
            
            _view.WriteLine($"{reservedAlive}-Cancelar");
        }

        public static void DisplayHasBeenSummoned(Unit newDemonAdded)
        {
            _view.WriteLine($"{newDemonAdded.GetName()} ha sido invocado");
        }

        public static void DisplayEmptySlot(List<int> validSlotsList, int iterator)
        {
            _view.WriteLine($"{validSlotsList.Count + 1}-Vacío (Puesto {iterator + 1})");
        }

        public static void DisplayDemonInSlot(List<int> validSlotsList, int iterator, Unit unit)
        {
            Stat currentStats = unit.GetCurrentStats();
            Stat baseStats = unit.GetBaseStats();

            _view.WriteLine($"{validSlotsList.Count + 1}-{unit.GetName()} " +
                            $"HP:{currentStats.GetStatByName("HP")}/{baseStats.GetStatByName("HP")} " +
                            $"MP:{currentStats.GetStatByName("MP")}/{baseStats.GetStatByName("MP")} (Puesto {iterator + 1})");
        }

        public static void ManageDisplayAffinity(string affinityType, AffinityContext affinityCtx, double finalDamage)
        {
            Unit target = affinityCtx.Target;
            Unit caster = affinityCtx.Caster;
                
            if (affinityType == "Rp")
            {
                DisplayFinalHP(caster);
                DisplaySeparator();
            }
            else if (affinityType == "Nu" || affinityType == "Dr")
            {
                DisplayFinalHP(target);
                DisplaySeparator();
            }
            else
            {
                DisplayFullDamageResult(target, finalDamage);
            }
        }

        public static void DisplayCombatUiForSkill(SkillUseContext skillCtx, AffinityContext affinityCtx, int numHits)
        {
            string affinityType = AffinityResolver.GetAffinity(affinityCtx.Target, affinityCtx.AttackType);
            double finalDamage = AffinityEffectManager.GetDamageBasedOnAffinity(affinityCtx);
    
            Skill skillInUse = skillCtx.Skill;
            bool isTargetAlly = skillInUse.Target == "Ally";
    
            if (isTargetAlly)
            {
                Console.WriteLine("ALLY AAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                DisplaySkillUsage(skillCtx.Caster, skillCtx.Skill, skillCtx.Target);
                double amountHealed = SkillManager.CalculateHeal(skillCtx.Target, skillCtx);
                DisplayHealing(skillCtx.Target, amountHealed);
            }
            else if (numHits == 1)
            {
                Console.WriteLine("NUM HITS 1!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                DisplaySkillUsage(skillCtx.Caster, skillCtx.Skill, skillCtx.Target); 
                DisplayAffinityMessage(affinityCtx);
                ManageDisplayAffinity(affinityType, affinityCtx, finalDamage);
            }
            else
            {
                Console.WriteLine("HAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                for (int i = 0; i < numHits; i++)
                {
                    DisplaySkillUsage(skillCtx.Caster, skillCtx.Skill, skillCtx.Target); 
                    DisplayAffinityMessage(affinityCtx);
                    if (finalDamage > 0) DisplayDamageTaken(affinityCtx.Target, finalDamage);
                }

                if (finalDamage >= 0)
                {
                    DisplayFinalHP(affinityCtx.Target);
                }
                else
                {
                    DisplayFinalHP(affinityCtx.Caster);
                }
                DisplaySeparator();
            }
        }
}       