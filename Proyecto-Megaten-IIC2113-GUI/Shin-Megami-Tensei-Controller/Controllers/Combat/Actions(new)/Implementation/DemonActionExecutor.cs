// using Shin_Megami_Tensei.Controllers.Exceptions;
// using Shin_Megami_Tensei.Enums;
//
// namespace Shin_Megami_Tensei.Managers.New_Actions.Implementation;
//
// public class DemonActionExecutor : IDemonActionExecutor
// {
//     private readonly IAttackExecutor _attackExecutor;
//     private readonly ISkillExecutor _skillExecutor;
//     private readonly ISummonExecutor _summonExecutor;
//     private readonly ITurnExecutor _turnExecutor;
//
//     public DemonActionExecutor(
//         IAttackExecutor attackExecutor,
//         ISkillExecutor skillExecutor,
//         ISummonExecutor summonExecutor,
//         ITurnExecutor turnExecutor)
//     {
//         _attackExecutor = attackExecutor ?? throw new ArgumentNullException(nameof(attackExecutor));
//         _skillExecutor = skillExecutor ?? throw new ArgumentNullException(nameof(skillExecutor));
//         _summonExecutor = summonExecutor ?? throw new ArgumentNullException(nameof(summonExecutor));
//         _turnExecutor = turnExecutor ?? throw new ArgumentNullException(nameof(turnExecutor));
//     }
//
//     public bool Execute(Demon demon, DemonAction action, Player currentPlayer)
//     {
//         return action switch
//         {
//             DemonAction.Attack => _attackExecutor.ExecutePhysicalAttack(demon, currentPlayer),
//             DemonAction.UseSkill => _skillExecutor.ExecuteSkill(demon, currentPlayer),
//             DemonAction.Summon => _summonExecutor.ExecuteSummon(demon, currentPlayer),
//             DemonAction.PassTurn => _turnExecutor.PassTurn(currentPlayer),
//             _ => throw new InvalidActionException($"Unknown demon action: {action}")
//         };
//     }
// }