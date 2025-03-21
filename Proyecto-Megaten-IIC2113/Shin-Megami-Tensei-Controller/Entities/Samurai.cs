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

    private Dictionary<string, int> ParseStats(string samuraiStats)
    {
        Dictionary<string, int> stats = new Dictionary<string, int>();
        string[] statPairs = samuraiStats.Split(',');

        foreach (string statPair in statPairs)
        {
            string[] keyValue = statPair.Split(':');
            if (keyValue.Length == 2)
            {
                string statName = keyValue[0].Trim(new char[] { '{', '}', ' ', '\"' });
                int statValue = int.Parse(keyValue[1].Trim(new char[] { '{', '}', ' ', '\"', '(', ')' }));
                stats[statName] = statValue;
            }
        }

        return stats;
    }

    public void SetAbilities(Skill newSkill)
    {
    }

    public override void UpdateStatsFromJSON()
    {
    }
}
