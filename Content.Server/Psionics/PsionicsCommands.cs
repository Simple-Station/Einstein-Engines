using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Abilities.Psionics;
using Robust.Shared.Console;
using Robust.Shared.Player;
using Content.Server.Abilities.Psionics;
using Robust.Shared.Prototypes;
using Content.Shared.Psionics;

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

[AdminCommand(AdminFlags.Fun)]
public sealed class AddPsionicPowerCommand : IConsoleCommand
{
    public string Command => "addpsionicpower";
    public string Description => Loc.GetString("command-addpsionicpower-description");
    public string Help => Loc.GetString("command-addpsionicpower-help");
    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var psionicPowers = entMan.System<PsionicAbilitiesSystem>();
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("shell-need-exactly-one-argument"));
            return;
        }

        if (!EntityUid.TryParse(args[0], out var uid))
        {
            shell.WriteError(Loc.GetString("addpsionicpower-args-one-error"));
            return;
        }

        if (!protoMan.TryIndex<PsionicPowerPrototype>(args[1], out var powerProto))
        {
            shell.WriteError(Loc.GetString("addpsionicpower-args-two-error"));
            return;
        }

        entMan.EnsureComponent<PsionicComponent>(uid, out var psionic);
        psionicPowers.InitializePsionicPower(uid, powerProto, psionic);
    }
}
