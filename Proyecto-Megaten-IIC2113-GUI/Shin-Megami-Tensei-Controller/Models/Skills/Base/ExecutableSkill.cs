using Shin_Megami_Tensei.Controllers.Exceptions;
using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers.Base;

public class ExecutableSkill
    {
        private readonly ITargetSelector _targetSelector;
        private readonly ISkillEffect _effect;
        private readonly Skill _skill;

        public ExecutableSkill(Skill skill, ITargetSelector targetSelector, ISkillEffect effect)
        {
            _skill = skill ?? throw new ArgumentNullException(nameof(skill));
            _targetSelector = targetSelector ?? throw new ArgumentNullException(nameof(targetSelector));
            _effect = effect ?? throw new ArgumentNullException(nameof(effect));
        }

        public string Name => _skill.Name;
        public int Cost => _skill.Cost;
        public Skill Skill => _skill;

        public bool CanExecute(SkillExecutionContext context)
        {
            if (!HasSufficientMana(context))
                return false;

            if (!_targetSelector.HasValidTargets(context))
                return false;

            return _effect.CanApply(context);
        }

        public void Execute(SkillExecutionContext context)
        {
            if (!CanExecute(context))
                throw new InvalidActionException($"Cannot execute skill {Name}");

            var targets = _targetSelector.SelectTargets(context);
            
            foreach (var target in targets)
            {
                var targetContext = new SkillExecutionContext(
                    context.Caster, 
                    context.Skill, 
                    context.CasterPlayer, 
                    context.OpponentPlayer, 
                    context.TurnContext
                );
                
                _effect.Apply(targetContext);
            }

            ConsumeMana(context);
        }

        private bool HasSufficientMana(SkillExecutionContext context)
        {
            var currentMana = context.Caster.GetCurrentStats().GetStatByName(StatType.Mp.ToGameString());
            return currentMana >= _skill.Cost;
        }

        private void ConsumeMana(SkillExecutionContext context)
        {
            var currentMana = context.Caster.GetCurrentStats().GetStatByName(StatType.Mp.ToGameString());
            var newMana = Math.Max(0, currentMana - _skill.Cost);
            context.Caster.GetCurrentStats().SetStatByName(StatType.Mp.ToGameString(), newMana);
        }
    }