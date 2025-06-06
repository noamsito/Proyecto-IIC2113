﻿using Shin_Megami_Tensei_View;
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

        public static string GetUserInputWithSeparator()
        {
            string input = _view.ReadLine();
            DisplaySeparator();
            return input;
        }
        
        public static void DisplaySeparator()
        {
            _view.WriteLine(GameConstants.Separator);
        }

        public static void DisplayActionSelection(string unitName)
        {
            _view.WriteLine($"Seleccione una acción para {unitName}");
        }

        public static void DisplaySamuraiOptions()
        {
            _view.WriteLine("1: Atacar");
            _view.WriteLine("2: Disparar");
            _view.WriteLine("3: Usar Habilidad");
            _view.WriteLine("4: Invocar");
            _view.WriteLine("5: Pasar Turno");
            _view.WriteLine("6: Rendirse");
        }

        public static void DisplayDemonOptions()
        {
            _view.WriteLine("1: Atacar");
            _view.WriteLine("2: Usar Habilidad");
            _view.WriteLine("3: Invocar");
            _view.WriteLine("4: Pasar Turno");
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
            PlayerUnitManager unitManagerPlayer = player.UnitManager;
            var units = unitManagerPlayer.GetActiveUnits();
            char label = 'A';
        
            foreach (var unit in units)
            {
                DisplayUnitInfo(unit, player, label);
                label++;
            }
        }
        
        private static void DisplayUnitInfo(Unit unit, Player player, char label)
        {
            if (unit == null)
            {
                PrintEmptySlot(label);
            }
            else if (IsSamurai(unit, player))
            {
                PrintUnitStats(label, unit);
            }
            else if (IsUnitDead(unit))
            {
                PrintEmptySlot(label);
            }
            else
            {
                PrintUnitStats(label, unit);
            }
        }
        
        private static bool IsSamurai(Unit unit, Player player)
        {
            return unit == player.GetTeam().Samurai;
        }
        
        private static bool IsUnitDead(Unit unit)
        {
            return unit.GetCurrentStats().GetStatByName("HP") <= 0;
        }
        
        private static void PrintEmptySlot(char label)
        {
            _view.WriteLine($"{label}-");
        }
        
        private static void PrintUnitStats(char label, Unit unit)
        {
            var currentStats = unit.GetCurrentStats();
            var baseStats = unit.GetBaseStats();
            int hp = currentStats.GetStatByName("HP");
            int maxHp = baseStats.GetStatByName("HP");
            int mp = currentStats.GetStatByName("MP");
            int maxMp = baseStats.GetStatByName("MP");
            _view.WriteLine($"{label}-{unit.GetName()} HP:{hp}/{maxHp} MP:{mp}/{maxMp}");
        }

        public static void DisplayUnitsGiven(List<Unit> deadUnits)
        {
            for (int i = 0; i < deadUnits.Count; i++)
            {
                Unit unit = deadUnits[i];
                string statusInfo = TargetSelector.FormatUnitStatus(unit);
                _view.WriteLine($"{i + 1}-{unit.GetName()} {statusInfo}");
            }
        }

        public static void DisplayTurnInfo(Player player)
        {
            PlayerTurnManager turnManagerPlayer = player.TurnManager;
            
            _view.WriteLine($"Full Turns: {turnManagerPlayer.GetFullTurns()}");
            _view.WriteLine($"Blinking Turns: {turnManagerPlayer.GetBlinkingTurns()}");
            _view.WriteLine(GameConstants.Separator);
        }

        public static void DisplayRoundPlayer(Samurai samurai, int playerNumber)
        {
            _view.WriteLine($"Ronda de {samurai.GetName()} (J{playerNumber})");
            _view.WriteLine(GameConstants.Separator);
        }
    
        public static void DisplaySortedUnits(Player player)
        {
            PlayerUnitManager unitManagerPlayer = player.UnitManager;   
            var units = unitManagerPlayer.GetSortedActiveUnitsByOrderOfAttack(); 
            
            _view.WriteLine("Orden:");

            int count = 0;
            foreach (var unit in units)
            {
                if (unit is not null)
                {
                    count++;
                    _view.WriteLine($"{count}-{unit.GetName()}");
                }
            }
            
            _view.WriteLine(GameConstants.Separator);
        }
    
        public static void DisplayWinner(Player winner)
        {
            int playerNumber = winner.GetName() == "Player 1" ? 1 : 2;
            _view.WriteLine($"Ganador: {winner.GetTeam().Samurai.GetName()} (J{playerNumber})");
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
        
        public static void DisplaySelectTarget(string attackerName)
        {
            _view.WriteLine($"Seleccione un objetivo para {attackerName}");
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

        private static int ConvertUnitToIntForDisplay(double number)
        {
            return Convert.ToInt32(Math.Floor(number));
        }
        
        private static void DisplayFullDamageResult(Unit target, double damage)
        {
            DisplayDamageTaken(target, damage);
            DisplayFinalHP(target);
            DisplaySeparator();
        }

        private static void DisplayDamageTaken(Unit target, double damage)
        {
            _view.WriteLine($"{target.GetName()} recibe {ConvertUnitToIntForDisplay(damage)} de daño");
        }

        public static void DisplayFinalHP(Unit target)
        {
            int currentHP = target.GetCurrentStats().GetStatByName("HP");
            int baseHP = target.GetBaseStats().GetStatByName("HP");
            _view.WriteLine($"{target.GetName()} termina con HP:{currentHP}/{baseHP}");
        }
    
        public static void DisplayHealingForMultiTargets(Unit caster, Unit target, double amountDamage)
        {
            int currentHp = target.GetCurrentStats().GetStatByName("HP");
            int baseHp = target.GetBaseStats().GetStatByName("HP");
            int amountHealed = ConvertUnitToIntForDisplay(amountDamage);
            
            _view.WriteLine($"{caster.GetName()} cura a {target.GetName()}");
            _view.WriteLine($"{target.GetName()} recibe {amountHealed} de HP");
            _view.WriteLine($"{target.GetName()} termina con HP:{currentHp}/{baseHp}");
        }
        
        public static void DisplayHealingForSingleTarget(Unit target, double amountDamage)
        {
            int currentHp = target.GetCurrentStats().GetStatByName("HP");
            int baseHp = target.GetBaseStats().GetStatByName("HP");
            int amountHealed = ConvertUnitToIntForDisplay(amountDamage);
            
            _view.WriteLine($"{target.GetName()} recibe {amountHealed} de HP");
            _view.WriteLine($"{target.GetName()} termina con HP:{currentHp}/{baseHp}");
        }
    
        public static void DisplayAffinityMessage(AffinityContext affinityCtx)
        {
            string affinityType = AffinityResolver.GetAffinity(affinityCtx.Target, affinityCtx.AttackType);
            string targetName = affinityCtx.Target.GetName();
            string attackerName = affinityCtx.Caster.GetName();
            int damage = ConvertUnitToIntForDisplay(affinityCtx.BaseDamage);
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
                "Light" => "ataca con luz a",
                "Dark" => "ataca con oscuridad a",
                "Almighty" => "lanza un ataque todo poderoso a",
                "Heal" when skill.Name is "Recarm" or "Samarecarm" or "Invitation" => "revive a",
                "Heal" => "cura a",
                _ => "usa " + skill.Name + " en"
            };
            
            _view.WriteLine($"{caster.GetName()} {action} {target.GetName()}");
        }

    
        public static void DisplayReviveForMultiTargets(Unit caster, Unit revived, double healAmount)
        {
            int amountHealed = ConvertUnitToIntForDisplay(healAmount);
            
            _view.WriteLine($"{caster.GetName()} revive a {revived.GetName()}");
            _view.WriteLine($"{revived.GetName()} recibe {amountHealed} de HP");
            _view.WriteLine($"{revived.GetName()} termina con HP:{revived.GetCurrentStats().GetStatByName("HP")}/{revived.GetBaseStats().GetStatByName("HP")}");
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
                DisplaySkillUsage(skillCtx.Caster, skillCtx.Skill, skillCtx.Target);
                double amountHealed = HealSkillsManager.CalculateHeal(skillCtx.Target, skillCtx);
                DisplayHealingForSingleTarget(skillCtx.Target, amountHealed);
                DisplaySeparator();
            }
            else if (numHits == 1)
            {
                DisplaySkillUsage(skillCtx.Caster, skillCtx.Skill, skillCtx.Target); 
                DisplayAffinityMessage(affinityCtx);
                ManageDisplayAffinity(affinityType, affinityCtx, finalDamage);
            }
            else
            {
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

        public static void DisplaySpecificForHealSkill(SkillUseContext skillCtx)
        {
            Unit unitCaster = skillCtx.Caster;
            
            switch (skillCtx.Skill.Name)
            {
                case "Recarmdra":
                    DisplayFinalHP(unitCaster);
                    break;
            }
        }

        public static void DisplaySkills(List<Skill> skills)
        {
            int displayIndex = 1;

            foreach (var skill in skills)
            {
                _view.WriteLine($"{displayIndex}-{skill.Name} MP:{skill.Cost}");
                displayIndex++;
            }
    
            _view.WriteLine($"{displayIndex}-Cancelar");
        }

        public static void DisplayUnitEliminated(Unit unit)
        {
            _view.WriteLine($"{unit.GetName()} ha sido eliminado");
        }

        public static void DisplayHasMissed(Unit unit)
        {
            _view.WriteLine($"{unit.GetName()} ha fallado el ataque");
        }

        public static void DisplayWeakMessage(Unit target, Unit attacker)
        {
            _view.WriteLine($"{target.GetName()} es débil contra el ataque de {attacker.GetName()}");
        }

        public static void DisplayBlockMessage(Unit target, Unit attacker)
        {
            _view.WriteLine($"{target.GetName()} bloquea el ataque de {attacker.GetName()}");
        }

        public static void DisplayResistMessage(Unit target, Unit attacker)
        {
            _view.WriteLine($"{target.GetName()} es resistente el ataque de {attacker.GetName()}");
        }
        
        public static void DisplayDamageReceived(Unit target, int damage)
        {
            _view.WriteLine($"{target.GetName()} recibe {damage} de daño");
        }

        public static void DisplayDrainMessage(Unit target, int amount)
        {
            _view.WriteLine($"{target.GetName()} absorbe {amount} daño");
        }

        public static void DisplayRepelMessage(Unit target, Unit caster, int damage)
        {
            _view.WriteLine($"{target.GetName()} devuelve {damage} daño a {caster.GetName()}");
        }
        
        public static void DisplayDrainHPMessage(Unit target, int drainAmount)
        {
            Console.WriteLine($"El ataque drena {drainAmount} HP de {target.GetName()}");
        }

        public static void DisplayDrainMPMessage(Unit target, int drainAmount)
        {
            Console.WriteLine($"El ataque drena {drainAmount} MP de {target.GetName()}");
        }

        public static void DisplayFinalMP(Unit unit)
        {
            int currentMP = unit.GetCurrentStats().GetStatByName("MP");
            int maxMP = unit.GetBaseStats().GetStatByName("MP");
            Console.WriteLine($"{unit.GetName()} termina con MP:{currentMP}/{maxMP}");
        }
}       