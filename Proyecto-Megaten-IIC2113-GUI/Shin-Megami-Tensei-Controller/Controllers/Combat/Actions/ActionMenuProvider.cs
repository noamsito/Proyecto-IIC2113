public static class ActionMenuProvider
{
    public static void DisplaySamuraiMenu(string unitName)
    {
        CombatUI.DisplayActionSelection(unitName);
        CombatUI.DisplaySamuraiOptions();
    }

    public static void DisplayDemonMenu(string unitName)
    {
        CombatUI.DisplayActionSelection(unitName);
        CombatUI.DisplayDemonOptions();
    }
}

