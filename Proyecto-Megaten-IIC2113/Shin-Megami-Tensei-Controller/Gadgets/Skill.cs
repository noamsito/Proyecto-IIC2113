namespace Shin_Megami_Tensei.Gadgets;

public class Skill
{
    private string name;
    private string type;
    private int cost;
    private int power;
    private string target;
    private string hits;
    private string effect;

    public Skill(string name, string type, int cost, int power, string target, string hits, string effect)
    {
        this.name = name;
        this.type = type;
        this.cost = cost;
        this.power = power;
        this.target = target;
        this.hits = hits;
        this.effect = effect;
    }

    public string GetName()
    {
        return this.name;
    }
    
    public string GetType()
    {
        return this.type;
    }
    
    public int GetCost()
    {
        return this.cost;
    }
    
    public int GetPower()
    {
        return this.power;
    }
    
    public string GetTarget()
    {
        return this.target;
    }
    
    public string GetHits()
    {
        return this.hits;
    }
    
    public string GetEffect()
    {
        return this.effect;
    }
    
    public void UpdateHability()
    {
        
    }
}