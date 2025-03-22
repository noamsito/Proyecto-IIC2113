using System.Text.Json.Nodes;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei;

public class Samurai : Unit
{
    private List<Skill> _skills = new List<Skill>();
    private Stat _stats;
    public Dictionary<string, string> Affinity;
    private const string JSON_FILE_PATH = "data/samurai.json";
    

    public Samurai(string name) : base(name)
    {
    }

    public void AddSkills(string newSkill)
    {
    }

    public override void SetStatsFromJSON()
    {
        string jsonString = File.ReadAllText(JSON_FILE_PATH);
    
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
    
                this._stats = new Stat(hp, mp, str, skl, mag, spd, lck);
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
        Console.WriteLine($"HP: {this._stats.GetStatByName("HP")}"); 
        Console.WriteLine($"MP: {this._stats.GetStatByName("MP")}");
        Console.WriteLine($"Str: {this._stats.GetStatByName("Str")}");
        Console.WriteLine($"Skl: {this._stats.GetStatByName("Skl")}");
        Console.WriteLine($"Mag: {this._stats.GetStatByName("Mag")}");
        Console.WriteLine($"Spd: {this._stats.GetStatByName("Spd")}");
        Console.WriteLine($"Lck: {this._stats.GetStatByName("Lck")}");
    }

    public override void SetSkillsFromJSON(string stringWithSkills)
    {
        string jsonString = File.ReadAllText("data/skills.json");
    
        JsonDocument document = JsonDocument.Parse(jsonString);
        JsonElement root = document.RootElement;

        // List<string> listOfSkills = this.ConvertStringToList(stringWithSkills);
        // Console.WriteLine($"Skills: {listOfSkills}");
        
        // this.PrintSkills();
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
}
