using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei;

public class Team
{
    public Samurai Samurai { get; private set; }
    public List<Demon> Demons { get; private set; } = new();
    public bool IsValid { get; private set; }
    public bool SamuraiRepeated { get; private set; }

    public Team() => Samurai = null;

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

    public bool HasSamurai() => Samurai != null;

    public bool HasLessThanMaximumUnits()
    {
        return HasSamurai() && Demons.Count <= GameConstants.MAX_DEMONS;
    }

    public bool IsAnyDemonRepeated()
    {
        HashSet<string> demonNames = new();

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
        HashSet<string> skillNames = new();

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
}