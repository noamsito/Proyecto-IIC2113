namespace Shin_Megami_Tensei;

public class Player
{
    public string Name;
    public Team Team;

    public Player(string name)
    {
        this.Name = name;
        // this.team = team;
    }

    public void SetTeam(Team team)
    {
        // this.Team = team;
    }
}