using Shin_Megami_Tensei.Enums;

namespace Shin_Megami_Tensei.Gadgets;
    
    public class Skill
    {
        public string Name { get; set; }
        public AttackType Type { get; set; }
        public int Cost { get; set; }
        public int Power { get; set; }
        public SkillTarget Target { get; set; }
        public string Hits { get; set; }
        public string Effect { get; set; }
    
        public Skill(string name, AttackType type, int cost, int power, SkillTarget target, string hits, string effect)
        {
            Name = name;
            Type = type;
            Cost = cost;
            Power = power;
            Target = target;
            Hits = hits;
            Effect = effect;
        }
    }