using System;
using System.Collections.Generic;
using System.IO;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei
{
    public class Samurai : Unit
    {
        private List<Skill> _skills = new List<Skill>();
        private Stat _stats;
        private const string JSON_FILE_PATH = "data/samurai.json";

        public Samurai(string name) : base(name)
        {
        }

        public void AddSkills(string newSkill)
        {
        }

        public override void SetStatsFromJSON()
        {
            string jsonContent = File.ReadAllText(JSON_FILE_PATH);
            string samuraiName = this.GetName();
        
            int startIndex = jsonContent.IndexOf(samuraiName);
            if (startIndex != -1)
            {
                int endIndex = jsonContent.IndexOf("}", startIndex);
                string samuraiStats = jsonContent.Substring(startIndex, endIndex - startIndex);
        
                Dictionary<string, int> stats = ParseStats(samuraiStats);
        
                if (stats.ContainsKey("HP") && stats.ContainsKey("MP") && stats.ContainsKey("Str") &&
                    stats.ContainsKey("Skl") && stats.ContainsKey("Mag") && stats.ContainsKey("Spd") &&
                    stats.ContainsKey("Lck"))
                {
                    this._stats = new Stat(
                        stats["HP"], stats["MP"], stats["Str"], stats["Skl"], 
                        stats["Mag"], stats["Spd"], stats["Lck"]
                    );
                }
                else
                {
                    throw new KeyNotFoundException("One or more required keys are missing in the stats dictionary.");
                }
            }
            else
            {
                throw new KeyNotFoundException("Samurai name not found in the JSON content.");
            }
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
}