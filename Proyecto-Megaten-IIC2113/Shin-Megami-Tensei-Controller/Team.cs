using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei;

public class Team
{
    private Samurai _samurai;
    private List<Demon> _demons;
    private bool _valid;
    private bool _samuraiRepeated;
    private const int MAX_DEMONS = 7;
    private const int MAX_SKILLS_SAMURAI = 8;

    public Team()
    {
        this._samurai = null;
        this._demons = new List<Demon>();
    }

    public void AddSamurai(Samurai samurai)
    {
        this._samurai = samurai;
    }

    public void AddDemon(Demon newDemon)
    {
        this._demons.Add(newDemon);
    }

    public Samurai GetSamurai()
    {
        return this._samurai;
    }
    
    public List<Demon> GetDemons()
    {
        return this._demons;
    }

    public bool GetValidation()
    {
        return this._valid;
    }

    public bool GetIfSamuraiIsRepeated()
    {
        return this._samuraiRepeated;
    }
    
    public void SetTeamAsInvalid()
    {
        this._valid = false;
    }
    
    public void SetTeamAsValid()
    {
        this._valid = true;
    }
    
    public bool HasSamurai()
    {
        return this._samurai != null;
    }

    public void SetSamuraiRepeated()
    {
        this.SetTeamAsInvalid();
        this._samuraiRepeated = true;
    }

    public bool HasMinimumUnits()
    {
        return this.HasSamurai() && this._demons.Count <= MAX_DEMONS;
    }
    
    public bool HasMaximumUnits()
    {
        return this.HasSamurai() && this._demons.Count <= MAX_DEMONS;
    }

    public bool UnitRepeated()
    {
        HashSet<string> demonNames = new HashSet<string>();
    
        foreach (Demon demon in this._demons)
        {
            if (!demonNames.Add(demon.GetName()))
            {
                return true; 
            }
        }
    
        return false; 
    }

    public bool SamuraiWithLessThanMaxSkills()
    {
        return this._samurai.GetQuantityOfSkills() <= MAX_SKILLS_SAMURAI;
    }
    
    public bool SamuraiWithRepeatedHabilities()
    {
        HashSet<string> skillNames = new HashSet<string>();
    
        foreach (Skill skill in this._samurai.GetSkills())
        {
            if (!skillNames.Add(skill.GetName()))
            {
                return true; 
            }
        }
    
        return false; 
    }

    public List<Demon> GetSortedDemonsBySpeed()
    { 
        return this._demons.OrderByDescending(
            demon => demon.GetCurrentStats().GetStatByName("Spd")
            ).ToList();
    }
}