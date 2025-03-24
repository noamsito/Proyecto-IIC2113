using System.Text.Json.Nodes;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei;

public class Samurai : Unit
{
    private List<Skill> _skills = new List<Skill>();
    public Dictionary<string, string> Affinity;
    private const string JSON_FILE_SAMURAI = "data/samurai.json";
    private const string JSON_FILE_SKILLS = "data/samurai.json";
    

    public Samurai(string name) : base(name)
    {
    }

    public void AddSkills(string newSkill)
    {
    }

    public override void SetStatsFromJSON()
    {
        string jsonString = File.ReadAllText(JSON_FILE_SAMURAI);
    
        JsonDocument document = JsonDocument.Parse(jsonString);
        JsonElement root = document.RootElement;
    
        foreach (JsonElement samurai in root.EnumerateArray())
        {
            if (samurai.GetProperty("name").GetString() == this.GetName())
            {
                int hp = samurai.GetProperty("stats").GetProperty("HP").GetInt32();
                int mp = samurai.GetProperty("stats").GetProperty("MP").GetInt32();
                int str = samurai.GetProperty("stats").GetProperty("Str").GetInt32();
                int skl = samurai.GetProperty("stats").GetProperty("Skl").GetInt32();
                int mag = samurai.GetProperty("stats").GetProperty("Mag").GetInt32();
                int spd = samurai.GetProperty("stats").GetProperty("Spd").GetInt32();
                int lck = samurai.GetProperty("stats").GetProperty("Lck").GetInt32();
    
                this._baseStats = new Stat(hp, mp, str, skl, mag, spd, lck);
                this._currentStats = new Stat(hp, mp, str, skl, mag, spd, lck);
    
                this.Affinity = new Dictionary<string, string>();
                foreach (JsonProperty affinity in samurai.GetProperty("affinity").EnumerateObject())
                {
                    this.Affinity[affinity.Name] = affinity.Value.GetString();
                }
    
                break;
            }
        }
        
        // this.PrintStats();
    }
    
    public override void UpdateStatsFromJSON()
    {
    }
    
    public void PrintStats()
    {
        Console.WriteLine("Name: " + this.GetName() + "\n");
        Console.WriteLine($"HP: {this._baseStats.GetStatByName("HP")}"); 
        Console.WriteLine($"MP: {this._baseStats.GetStatByName("MP")}");
        Console.WriteLine($"Str: {this._baseStats.GetStatByName("Str")}");
        Console.WriteLine($"Skl: {this._baseStats.GetStatByName("Skl")}");
        Console.WriteLine($"Mag: {this._baseStats.GetStatByName("Mag")}");
        Console.WriteLine($"Spd: {this._baseStats.GetStatByName("Spd")}");
        Console.WriteLine($"Lck: {this._baseStats.GetStatByName("Lck")}");
        
        Console.WriteLine("Affinity:");
        foreach (var affinity in this.Affinity)
        {
            Console.WriteLine($"{affinity.Key}: {affinity.Value}");
        }
    }

    public void SetSamuraiSkillsFromJSON(List<string> skillsList)
    {
        string jsonString = File.ReadAllText(JSON_FILE_SKILLS);
    
        JsonDocument document = JsonDocument.Parse(jsonString);
        JsonElement root = document.RootElement; // cambiar nombre
        
        foreach (var skill in skillsList)
        {
            this.MatchListSkillsWithJSON(skill, root);
        }
        
        this.PrintSkills();
    }
    
    public void MatchListSkillsWithJSON(string skill, JsonElement root)
    {
        foreach (JsonElement skillJSON in root.EnumerateArray())
        {
            if (skillJSON.GetProperty("name").GetString() == skill)
            {
                string name = skillJSON.GetProperty("name").GetString();
                string type = skillJSON.GetProperty("type").GetString();
                int cost = skillJSON.GetProperty("cost").GetInt32();
                int power = skillJSON.GetProperty("power").GetInt32();
                string target = skillJSON.GetProperty("target").GetString();
                string hits = skillJSON.GetProperty("hits").GetString();
                string effect = skillJSON.GetProperty("effect").GetString();
    
                this.AssignSkillWithSamurai(name, type, cost, power, target, hits, effect);
                break;
            }
        }
    }

    public void AssignSkillWithSamurai(string name, string type, int cost, 
                                        int power, string target, string hits, string effect)
    {
        Skill newSkill = new Skill(name, type, cost, power, target, hits, effect);
        this._skills.Add(newSkill);
    }

    public List<string> ConvertStringToList(string skillsString)
    {
        string[] skillsArray = skillsString.Trim('(', ')').Split(',');
    
        return new List<string>(skillsArray);
    }
    
    public void PrintSkills()
    {
        Console.WriteLine("Name: " + this.GetName());
        Console.WriteLine("Skills:");
        foreach (var skill in this._skills)
        {
            Console.WriteLine($"Name: {skill.GetName()}");
            Console.WriteLine($"Type: {skill.GetType()}");
            Console.WriteLine($"Cost: {skill.GetCost()}");
            Console.WriteLine($"Power: {skill.GetPower()}");
            Console.WriteLine($"Target: {skill.GetTarget()}");
            Console.WriteLine($"Hits: {skill.GetHits()}");
            Console.WriteLine($"Effect: {skill.GetEffect()}");
            Console.WriteLine();
        }
    }

    public override void UpdateSkillsFromJSON()
    {
        
    }
    
    public int GetQuantityOfSkills()
    {
        return this._skills.Count;
    }
}
