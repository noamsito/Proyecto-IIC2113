using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;

public static class DemonActionHandler
{
    public static void Handle(Demon demon, CombatContext ctx)
    {
        bool actionExecuted = false;

        while (!actionExecuted)
        {
            ctx.View.WriteLine($"Seleccione una acción para {demon.GetName()}");
            ctx.View.WriteLine("1: Atacar\n2: Usar Habilidad\n3: Invocar\n4: Pasar Turno");

            string input = ctx.View.ReadLine();
            ctx.View.WriteLine(GameConstants.Separator);

            var actionCtx = new DemonActionContext(demon, ctx.CurrentPlayer, ctx.Opponent, ctx.View);
            actionExecuted = DemonActionExecutor.Execute(input, actionCtx);
        }
    }
}