using Shin_Megami_Tensei_View.Implementation.Interfaces;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Managers;

namespace Shin_Megami_Tensei_View.Implementation.Implementation;

public class AffinityDisplayer : IAffinityDisplayer
    {
        private readonly IDisplayService _displayService;

        public AffinityDisplayer(IDisplayService displayService)
        {
            _displayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
        }

        public void DisplayAffinityMessage(AffinityContext affinityContext)
        {
            var affinity = AffinityResolver.GetAffinity(affinityContext.Target, affinityContext.AttackType);
            var affinityType = AffinityTypeExtensions.FromGameString(affinity);
            
            string message = CreateAffinityMessage(affinityType, affinityContext);
            
            if (!string.IsNullOrEmpty(message))
                _displayService.WriteLine(message);
        }

        public void DisplayWeakMessage(Unit target, Unit attacker)
        {
            _displayService.WriteLine($"{target.GetName()} es débil contra el ataque de {attacker.GetName()}");
        }

        public void DisplayResistMessage(Unit target, Unit attacker)
        {
            _displayService.WriteLine($"{target.GetName()} es resistente el ataque de {attacker.GetName()}");
        }

        public void DisplayBlockMessage(Unit target, Unit attacker)
        {
            _displayService.WriteLine($"{target.GetName()} bloquea el ataque de {attacker.GetName()}");
        }

        public void DisplayRepelMessage(Unit target, Unit caster, int damage)
        {
            _displayService.WriteLine($"{target.GetName()} devuelve {damage} daño a {caster.GetName()}");
        }

        public void DisplayDrainMessage(Unit target, int amount)
        {
            _displayService.WriteLine($"{target.GetName()} absorbe {amount} daño");
        }

        private string CreateAffinityMessage(AffinityType affinityType, AffinityContext affinityContext)
        {
            string targetName = affinityContext.Target.GetName();
            string attackerName = affinityContext.Caster.GetName();
            int damage = ConvertToDisplayUnit(affinityContext.BaseDamage);

            return affinityType switch
            {
                AffinityType.Weak => $"{targetName} es débil contra el ataque de {attackerName}",
                AffinityType.Resistant => $"{targetName} es resistente el ataque de {attackerName}",
                AffinityType.Null => $"{targetName} bloquea el ataque de {attackerName}",
                AffinityType.Repel => $"{targetName} devuelve {damage} daño a {attackerName}",
                AffinityType.Drain => $"{targetName} absorbe {damage} daño",
                AffinityType.Normal => "",
                _ => ""
            };
        }

        private int ConvertToDisplayUnit(double number)
        {
            return Convert.ToInt32(Math.Floor(number));
        }
    }
