using System.Text.Json.Nodes;
using System.IO;
using Newtonsoft.Json.Linq;
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
        
        JArray samuraiArray = JArray.Parse(jsonString);
        foreach (JObject samurai in samuraiArray)
        {
            if (samurai["name"]?.ToString() == this.GetName())
            {
                int hp = samurai["stats"]?["HP"]?.Value<int>() ?? 0;
                int mp = samurai["stats"]?["MP"]?.Value<int>() ?? 0;
                int str = samurai["stats"]?["Str"]?.Value<int>() ?? 0;
                int skl = samurai["stats"]?["Skl"]?.Value<int>() ?? 0;
                int mag = samurai["stats"]?["Mag"]?.Value<int>() ?? 0;
                int spd = samurai["stats"]?["Spd"]?.Value<int>() ?? 0;
                int lck = samurai["stats"]?["Lck"]?.Value<int>() ?? 0;
    
                this._stats = new Stat(hp, mp, str, skl, mag, spd, lck);
                break;
            }
        }
        
        this.PrintStats();
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
