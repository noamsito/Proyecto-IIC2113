namespace Shin_Megami_Tensei.Managers;

public class PlayerTeamManager
{
    private readonly Player _player;

    public PlayerTeamManager(Player player)
    {
        _player = player;
    }

    public void OrganizeTeamUnits()
    {
        SetActiveUnits();
        SetReserveUnits();
    }

    public bool HasInvalidTeamConfiguration()
    {
        Team team = _player.GetTeam();
        return !team.HasSamurai() ||
               team.SamuraiRepeated ||
               !team.HasLessThanMaximumUnits() ||
               team.IsAnyDemonRepeated() ||
               team.HasSamuraiExceededMaxSkills() ||
               team.HasSamuraiRepeatedSkills();
    }

    public void SetActiveUnits()
    {
        Team team = _player.GetTeam();
        Samurai samurai = team.Samurai;
        List<Demon> demons = team.Demons;

        _player.UnitManager.SetSamuraiInActiveSlot(samurai);
        AssignDemonsToActiveSlots(demons);
    }

    private void AssignDemonsToActiveSlots(List<Demon> demons)
    {
        for (int i = 0; i < demons.Count && i < 3; i++)
        {
            _player.UnitManager.SetDemonInActiveSlot(i + 1, demons[i]);
        }
    }

    public void SetReserveUnits()
    {
        Team team = _player.GetTeam();
        List<Demon> demons = team.Demons;
        List<Unit> activeUnits = _player.UnitManager.GetActiveUnits();

        _player.UnitManager.ClearReservedUnits();

        foreach (var demon in demons)
        {
            if (demon != null && !activeUnits.Contains(demon))
            {
                _player.UnitManager.AddToReservedUnits(demon);
            }
        }
    }

    public void ReorderReserveBasedOnJsonOrder()
    {
        Team team = _player.GetTeam();
        List<Unit> reservedUnits = _player.UnitManager.GetReservedUnits();

        var orderedDemons = team.Demons
            .Where(demon => reservedUnits.Contains(demon))
            .ToList();

        _player.UnitManager.SetReservedUnits(orderedDemons.Cast<Unit>().ToList());
    }
}