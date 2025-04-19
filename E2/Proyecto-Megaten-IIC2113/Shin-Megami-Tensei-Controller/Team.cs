using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.String_Handlers;

namespace Shin_Megami_Tensei;
    
    public class Team
    {
        public Samurai Samurai { get; private set; }
        public List<Demon> Demons { get; private set; } = new List<Demon>();
        public bool IsValid { get; private set; }
        public bool SamuraiRepeated { get; private set; }
    
        public Team()
        {
            Samurai = null;
        }
    
        public void AddSamurai(Samurai samurai)
        {
            Samurai = samurai;
        }
    
        public void AddDemon(Demon newDemon)
        {
            Demons.Add(newDemon);
        }
    
        public void SetTeamAsInvalid()
        {
            IsValid = false;
        }
    
        public void SetTeamAsValid()
        {
            IsValid = true;
        }
    
        public void SetSamuraiRepeated()
        {
            SetTeamAsInvalid();
            SamuraiRepeated = true;
        }
    
        public bool HasSamurai()
        {
            return Samurai != null;
        }
    
        public bool HasLessThanMaximumUnits()
        {
            return HasSamurai() && Demons.Count <= GameConstants.MAX_DEMONS;
        }
    
        public bool IsAnyDemonRepeated()
        {
            HashSet<string> demonNames = new HashSet<string>();
    
            foreach (Demon demon in Demons)
            {
                if (!demonNames.Add(demon.GetName()))
                {
                    return true;
                }
            }
    
            return false;
        }
    
        public bool HasSamuraiExceededMaxSkills()
        {
            return Samurai.GetSkillCount() > GameConstants.MAX_SKILLS_SAMURAI;
        }
    
        public bool HasSamuraiRepeatedSkills()
        {
            HashSet<string> skillNames = new HashSet<string>();
    
            foreach (Skill skill in Samurai.GetSkills())
            {
                string skillName = skill.Name.ToLower().Trim();
                if (!skillNames.Add(skillName))
                {
                    return true;
                }
            }
    
            return false;
        }
        
        public void ConvertStringToTeam(List<string> teamUnitDescriptions)
        {
            foreach (var unitDescription in teamUnitDescriptions)
            {
                if (StringHelper.IsSamuraiUnit(unitDescription))
                {
                    TryAddSamuraiToTeam(unitDescription);
                }
                else
                {
                    AddDemonToTeam(unitDescription);
                }
            }
        }
    
        private void TryAddSamuraiToTeam(string samuraiDescription)
        {
            if (this.HasSamurai())
            {
                this.SetSamuraiRepeated();
                return;
            }
        
            var (samuraiName, samuraiSkills) = StringHelper.ExtractSamuraiNameAndSkills(samuraiDescription);
            Samurai newSamurai = CreateSamuraiWithSkills(samuraiName, samuraiSkills);
            this.AddSamurai(newSamurai);
        }

        private void AddDemonToTeam(string unit)
        {
            string demonName = unit.Trim();
            Demon demon = new Demon(demonName); 
            this.AddDemon(demon);
        }

        private Samurai CreateSamuraiWithSkills(string name, List<string> skills)
        {
            Samurai samurai = new Samurai(name, skills);
            return samurai;
        }
    }