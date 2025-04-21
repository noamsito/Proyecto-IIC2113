namespace Shin_Megami_Tensei.String_Handlers;

public static class StringHelper
{
    public static (string SamuraiName, List<string> Skills) ExtractSamuraiNameAndSkills(string unitDescription)
    {
        const string SamuraiPrefix = "[Samurai]";
        List<string> samuraiSkills = new List<string>();
        int nameStartPosition = SamuraiPrefix.Length;
        int openParenthesisPosition = unitDescription.IndexOf('(');
        string extractedName;
        
        if (HasSkillsList(openParenthesisPosition))
        {
            extractedName = ExtractNameWithSkills(unitDescription, nameStartPosition, openParenthesisPosition);
            samuraiSkills = ExtractSkillsList(unitDescription, openParenthesisPosition);
        }
        else
        {
            extractedName = ExtractNameWithoutSkills(unitDescription, nameStartPosition);
        }
    
        return (extractedName, samuraiSkills);
    }
    
    private static string ExtractNameWithSkills(string text, int startPosition, int endPosition)
    {
        return text.Substring(startPosition, endPosition - startPosition).Trim();
    }
    
    private static List<string> ExtractSkillsList(string text, int openParenthesisPosition)
    {
        int closeParenthesisPosition = text.IndexOf(')', openParenthesisPosition);
        if (closeParenthesisPosition == -1)
        {
            return new List<string>();
        }
        
        string skillsText = text.Substring(openParenthesisPosition + 1, 
            closeParenthesisPosition - openParenthesisPosition - 1);
        
        return skillsText.Split(',')
            .Select(skill => skill.Trim())
            .ToList();
    }


    private static bool HasSkillsList(int parenthesisPosition)
    {
        return parenthesisPosition != -1;
    }

    private static string ExtractNameWithoutSkills(string text, int startPosition)
    {
        return text.Substring(startPosition).Trim();
    }
    
    public static bool IsSamuraiUnit(string unitDescription)
    {
        return unitDescription.StartsWith("[Samurai]");
    }
    
    public static List<string> GetNonEmptyLines(string content)
    {
        return content.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
    }
    
    public static string GetPlayerIdentifierFromHeaderLine(string line)
    {
        if (IsTeamHeaderForPlayer(line, "Player 1 Team"))
        {
            return "Player 1";
        }
        
        if (IsTeamHeaderForPlayer(line, "Player 2 Team"))
        {
            return "Player 2";
        }
        
        return null;
    }
    
    private static bool IsTeamHeaderForPlayer(string line, string playerTeamHeader)
    {
        return line.StartsWith(playerTeamHeader);
    }
}