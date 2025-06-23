// using Shin_Megami_Tensei.Controllers.Exceptions;
// using Shin_Megami_Tensei.Enums;
//
// namespace Shin_Megami_Tensei.Managers.New_Actions.Implementation;
//
// public class SamuraiActionExecutor : ISamuraiActionExecutor
// {
//     private readonly IAttackExecutor _attackExecutor;
//     private readonly ISkillExecutor _skillExecutor;
//     private readonly ISummonExecutor _summonExecutor;
//     private readonly ITurnExecutor _turnExecutor;
//
//     public SamuraiActionExecutor(
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
//     public bool Execute(Samurai samurai, SamuraiAction action, Player currentPlayer)
//     {
//         return action switch
//         {
//             SamuraiAction.Attack => _attackExecutor.ExecutePhysicalAttack(samurai, currentPlayer),
//             SamuraiAction.Shoot => _attackExecutor.ExecuteGunAttack(samurai, currentPlayer),
//             SamuraiAction.UseSkill => _skillExecutor.ExecuteSkill(samurai, currentPlayer),
//             SamuraiAction.Summon => _summonExecutor.ExecuteSummon(samurai, currentPlayer),
//             SamuraiAction.PassTurn => _turnExecutor.PassTurn(currentPlayer),
//             SamuraiAction.Surrender => _turnExecutor.Surrender(currentPlayer),
//             _ => throw new InvalidActionException($"Unknown samurai action: {action}")
//         };
//     }
// }
