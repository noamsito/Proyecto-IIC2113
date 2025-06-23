using Shin_Megami_Tensei_View.Implementation.Interfaces;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei_View.Implementation.Implementation;

public class CombatDisplayer : ICombatDisplayer
    {
        private readonly IDisplayService _displayService;

        public CombatDisplayer(IDisplayService displayService)
        {
            _displayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
        }

        public void DisplayActionSelection(string unitName)
        {
            _displayService.WriteLine($"Seleccione una acción para {unitName}");
        }

        public void DisplaySamuraiOptions()
        {
            _displayService.WriteLine("1: Atacar");
            _displayService.WriteLine("2: Disparar");
            _displayService.WriteLine("3: Usar Habilidad");
            _displayService.WriteLine("4: Invocar");
            _displayService.WriteLine("5: Pasar Turno");
            _displayService.WriteLine("6: Rendirse");
        }

        public void DisplayDemonOptions()
        {
            _displayService.WriteLine("1: Atacar");
            _displayService.WriteLine("2: Usar Habilidad");
            _displayService.WriteLine("3: Invocar");
            _displayService.WriteLine("4: Pasar Turno");
        }

        public void DisplayAttack(string attackerName, string targetName, AttackType attackType)
        {
            string action = GetAttackActionText(attackType);
            _displayService.WriteLine($"{attackerName} {action} {targetName}");
        }

        public void DisplaySkillUsage(Unit caster, Skill skill, Unit target)
        {
            string action = GetSkillActionText(skill);
            _displayService.WriteLine($"{caster.GetName()} {action} {target.GetName()}");
        }

        private string GetAttackActionText(AttackType attackType)
        {
            return attackType switch
            {
                AttackType.Physical => "ataca a",
                AttackType.Gun => "dispara a",
                AttackType.Fire => "lanza fuego a",
                AttackType.Ice => "lanza hielo a",
                AttackType.Electric => "lanza electricidad a",
                AttackType.Force => "lanza viento a",
                _ => "ataca a"
            };
        }

        private string GetSkillActionText(Skill skill)
        {
            var attackType = skill.Type;
            return attackType switch
            {
                AttackType.Fire => "lanza fuego a",
                AttackType.Ice => "lanza hielo a",
                AttackType.Electric => "lanza electricidad a",
                AttackType.Force => "lanza viento a",
                AttackType.Physical => "ataca a",
                AttackType.Gun => "dispara a",
                AttackType.Light => "ataca con luz a",
                AttackType.Dark => "ataca con oscuridad a",
                AttackType.Almighty => "lanza un ataque todo poderoso a",
                AttackType.Heal when IsReviveSkill(skill.Name) => "revive a",
                AttackType.Heal => "cura a",
                _ => "usa " + skill.Name + " en"
            };
        }

        private bool IsReviveSkill(string skillName)
        {
            return skillName is "Recarm" or "Samarecarm" or "Invitation";
        }
    }
