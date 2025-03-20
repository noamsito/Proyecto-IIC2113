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

    public bool MinimumUnits()
    {
        
        
        return true;
    }

    public bool UnitNotRepeated()
    {
        
        
        return true;
    }

    public bool SamuraiHasEnoughHabilities()    
    {
        
        return true;
    }

    public bool SamuraiHabilityNotRepeated()
    {
        return true;
    }
}