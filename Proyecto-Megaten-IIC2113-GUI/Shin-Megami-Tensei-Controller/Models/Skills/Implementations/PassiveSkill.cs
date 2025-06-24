// using Shin_Megami_Tensei_GUI;
// using Shin_Megami_Tensei.Enums;
// using Shin_Megami_Tensei.Gadgets;
//
// namespace Shin_Megami_Tensei.Models.Skills
// {
//     public class PassiveSkill : Skill
//     {
//         public PassiveSkill(string name) 
//             : base(name, AttackType.Passive, 0, 0, 0, "0", SkillTarget.Self) 
//         {
//         }
//         
//         public override bool CanExecute(IUnit user)
//         {
//             // Las skills pasivas siempre están activas
//             return false; // No se pueden ejecutar manualmente
//         }
//         
//         public override void Execute(IUnit user, IUnit target)
//         {
//             // Las skills pasivas no se ejecutan, aplican efectos constantes
//             throw new InvalidOperationException("Passive skills cannot be executed");
//         }
//     }
// }