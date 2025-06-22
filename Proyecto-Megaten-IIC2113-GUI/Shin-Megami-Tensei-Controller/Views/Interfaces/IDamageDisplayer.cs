using Shin_Megami_Tensei;

namespace Shin_Megami_Tensei_View.Implementation.Interfaces;

public interface IDamageDisplayer
{
    void DisplayDamageReceived(Unit target, int damage);
    void DisplayFinalHP(Unit target);
    void DisplayFinalMP(Unit unit);
    void DisplayUnitEliminated(Unit unit);
    void DisplayHasMissed(Unit unit);
}