namespace Shin_Megami_Tensei;

public class Team
{
    private Samurai samurai;
    private List<Demon> demons;
    private bool valid;

    public Team()
    {
        this.samurai = null;
        this.demons = new List<Demon>();
    }

    public void AddSamurai(Samurai samurai)
    {
        this.samurai = samurai;
    }

    public void AddDemon(Demon newDemon)
    {
        this.demons.Add(newDemon);
    }

    public Samurai GetSamurai()
    {
        return this.samurai;
    }
    
    public List<Demon> GetDemons()
    {
        return this.demons;
    }

    public void SetTeamAsInvalid()
    {
        this.valid = false;
    }
    
    public bool HasSamurai()
    {
        return this.samurai != null;
    }

    public bool HasMinimumUnits()
    {
        return this.HasSamurai() && this.demons.Count <= 7;
    }

    public bool UnitNotRepeated()
    {
        HashSet<string> demonNames = new HashSet<string>();
    
        foreach (Demon demon in this.demons)
        {
            if (!demonNames.Add(demon.Name))
            {
                return false; 
            }
        }
    
        return true; 
    }
}