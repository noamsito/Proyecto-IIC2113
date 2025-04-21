using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Controllers;
using Shin_Megami_Tensei.Managers;

public static class SamuraiActionHandler
{
    public static void Handle(Samurai samurai, CombatContext ctx)
    {
        bool actionExecuted = false;

        while (!actionExecuted)
        {
            ctx.View.WriteLine($"Seleccione una acción para {samurai.GetName()}");
            ctx.View.WriteLine("1: Atacar\n2: Disparar\n3: Usar Habilidad\n4: Invocar\n5: Pasar Turno\n6: Rendirse");

            string input = ctx.View.ReadLine();
            ctx.View.WriteLine(GameConstants.Separator);

            var actionCtx = new SamuraiActionContext(samurai, ctx.CurrentPlayer, ctx.Opponent, ctx.View);
            actionExecuted = SamuraiActionExecutor.Execute(input, actionCtx);
        }
    }
}