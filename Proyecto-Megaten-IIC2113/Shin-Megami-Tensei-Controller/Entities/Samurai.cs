using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Nodes;
// using System.Text.Json.;
using System.Text.Json.Serialization;
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
        string jsonString = File.ReadAllText("data/samurai.json");
        
        JsonObject JSONSamurai = 
    }


    
    public void PrintStats()
    {
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
