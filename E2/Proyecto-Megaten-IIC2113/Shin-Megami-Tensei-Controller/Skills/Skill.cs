namespace Shin_Megami_Tensei.Gadgets;
    
    public class Skill
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Cost { get; set; }
        public int Power { get; set; }
        public string Target { get; set; }
        public string Hits { get; set; }
        public string Effect { get; set; }
    
        public Skill(string name, string type, int cost, int power, string target, string hits, string effect)
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