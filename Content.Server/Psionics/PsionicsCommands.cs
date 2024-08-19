using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Abilities.Psionics;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Server.Psionics;

[AdminCommand(AdminFlags.Logs)]
public sealed class ListPsionicsCommand : IConsoleCommand
{
    public string Command => "lspsionics";
    public string Description => Loc.GetString("command-lspsionic-description");
    public string Help => Loc.GetString("command-lspsionic-help");
    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        foreach (var (actor, psionic, meta) in entMan.EntityQuery<ActorComponent, PsionicComponent, MetaDataComponent>())
        {
            var powerList = new List<string>();
            foreach (var power in psionic.ActivePowers)
                powerList.Add(power.Name);

            shell.WriteLine(meta.EntityName + " (" + meta.Owner + ") - " + actor.PlayerSession.Name + powerList);
        }
    }
}
