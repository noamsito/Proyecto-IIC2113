using Shin_Megami_Tensei.Controllers.Exceptions;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Controllers.Collections;

public class SkillCollection
{
    private readonly List<Skill> _skills;
    private readonly int _maxSkills;

    public SkillCollection(int maxSkills = int.MaxValue)
    {
        _skills = new List<Skill>();
        _maxSkills = maxSkills;
    }

    public int Count => _skills.Count;
    public bool IsFull => _skills.Count >= _maxSkills;

    public void AddSkill(Skill skill)
    {
        if (skill == null)
            throw new ArgumentNullException(nameof(skill));

        if (IsFull)
            throw new InvalidOperationException($"Cannot add more than {_maxSkills} skills");

        if (_skills.Any(s => s.Name.Equals(skill.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Skill '{skill.Name}' already exists");

        _skills.Add(skill);
    }

    public IReadOnlyList<Skill> GetAllSkills()
    {
        return _skills.AsReadOnly();
    }

    public IReadOnlyList<Skill> GetAffordableSkills(int availableMana)
    {
        return _skills.Where(s => s.Cost <= availableMana).ToList().AsReadOnly();
    }

    public Skill GetSkillByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Skill name cannot be null or empty", nameof(name));

        var skill = _skills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (skill == null)
            throw new SkillNotFoundException(name);

        return skill;
    }

    public bool TryGetSkillByName(string name, out Skill skill)
    {
        skill = null;
        if (string.IsNullOrWhiteSpace(name))
            return false;

        skill = _skills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return skill != null;
    }

    public bool HasSkill(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && 
               _skills.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void RemoveSkill(string name)
    {
        var skill = GetSkillByName(name);
        _skills.Remove(skill);
    }

    public bool TryRemoveSkill(string name)
    {
        if (TryGetSkillByName(name, out var skill))
        {
            _skills.Remove(skill);
            return true;
        }
        return false;
    }
}
