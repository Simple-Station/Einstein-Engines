using Content.Shared._Crescent.Diplomacy;
using Robust.Shared.Console;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Shuttles.Components;


namespace Content.Server._Crescent.Diplomacy;

public partial class DiplomacySystem
{
    [Dependency] private readonly IConsoleHost _conHost = default!;

    public void InitializeCommands()
    {
        _conHost.RegisterCommand("getfactionrelations", "Gets relations for a given faction", "getfactionrelations <faction ID>",
            GetFactionRelationsCmd,
            GetFactionRelationsCompletion);

        _conHost.RegisterCommand("changefactionrelations", "Changes relations between 2 factions", "changefactionrelations <faction 1 ID> <faction 2 ID> <new relation>",
            ChangeFactionRelationsCmd,
            ChangeFactionRelationsCompletion);

        _conHost.RegisterCommand("changeifffaction", "Changes the faction of a UID with IFF", "changeifffaction <uid> <faction ID>",
            ChangeIFFFactionCmd,
            ChangeIFFFactionCompletion);
    }

    [AdminCommand(AdminFlags.Logs)]
    private void GetFactionRelationsCmd(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError("ONE ARGUMENT. ONE FACTION BUDDY!");
            return;
        }

        var dict = GetRelationsForFaction(args[0]);

        foreach (var relation in dict)
        {
            shell.WriteLine(relation.Key + ": " + relation.Value);
        }
    }

    public CompletionResult GetFactionRelationsCompletion(IConsoleShell shell, string[] args)
    {
        switch (args.Length)
        {
            case 1:
                var opts = CompletionHelper.PrototypeIDs<DiplomacyPrototype>();
                return CompletionResult.FromOptions(opts);
        }

        return CompletionResult.Empty;
    }

    [AdminCommand(AdminFlags.Fun)]
    private void ChangeFactionRelationsCmd(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 3)
        {
            shell.WriteError("THREE. THERE SHALL BE... THREE ARGUMENTS!");
            return;
        }

        if (!_prototypeManager.TryIndex<DiplomacyPrototype>(args[0], out var _))
        {
            shell.WriteError(args[0] + " is not a faction.");
            return;
        }

        if (!_prototypeManager.TryIndex<DiplomacyPrototype>(args[1], out var _))
        {
            shell.WriteError(args[1] + " is not a faction.");
            return;
        }

        if (!Enum.TryParse(args[2], out Relations relations))
        {
            shell.WriteError(args[2] + " is not a relation.");
            return;
        }

        ChangeRelation(args[0], args[1], relations);
        shell.WriteLine("Relations between " + args[0] + " and " + args[1] + " are now " + GetRelations(args[0], args[1]));
    }

    public CompletionResult ChangeFactionRelationsCompletion(IConsoleShell shell, string[] args)
    {
        switch (args.Length)
        {
            case 1:
                var opts = CompletionHelper.PrototypeIDs<DiplomacyPrototype>();
                return CompletionResult.FromOptions(opts);
            case 2:
                var opts2 = CompletionHelper.PrototypeIDs<DiplomacyPrototype>();
                return CompletionResult.FromOptions(opts2);
            case 3:
                return CompletionResult.FromHint("Ally, Neutral, ColdWar, War");
        }

        return CompletionResult.Empty;
    }

    [AdminCommand(AdminFlags.Fun)]
    private void ChangeIFFFactionCmd(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError("Two arguments.");
            return;
        }

        if (!EntityUid.TryParse(args[0], out var uid))
        {
            shell.WriteError("Invalid UID.");
            return;
        }

        _shuttleSystem.SetIFFFaction(uid, args[1]);
    }

    public CompletionResult ChangeIFFFactionCompletion(IConsoleShell shell, string[] args)
    {
        switch (args.Length)
        {
            case 1:
                return CompletionResult.FromHint("A grid with an IFF component");
            case 2:
                var opts2 = CompletionHelper.PrototypeIDs<DiplomacyPrototype>();
                return CompletionResult.FromOptions(opts2);
        }

        return CompletionResult.Empty;
    }

}
