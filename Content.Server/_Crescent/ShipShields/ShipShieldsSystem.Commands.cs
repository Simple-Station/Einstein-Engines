
using Robust.Shared.Console;
using Content.Server.Administration;
using Content.Shared.Administration;


namespace Content.Server._Crescent.ShipShields;
public partial class ShipShieldsSystem
{
    [Dependency] private readonly IConsoleHost _conHost = default!;

    public void InitializeCommands()
    {
        _conHost.RegisterCommand("shieldentity", "Create a shield around an entity", "shieldentity <uid>",
            ShieldEntityCmd);
        _conHost.RegisterCommand("unshieldentity", "Remove a shield from an entity", "unshieldentity <uid>",
            UnshieldEntityCmd);
    }

    [AdminCommand(AdminFlags.Debug)]
    public void ShieldEntityCmd(IConsoleShell shell, string argstr, string[] args)
    {
        if (!EntityUid.TryParse(args[0], out var uid))
        {
            shell.WriteError("Couldn't parse entity.");
            return;
        }

        var shield = ShieldEntity(uid);

        shell.WriteLine("Created shield " + shield);
    }

    [AdminCommand(AdminFlags.Debug)]
    public void UnshieldEntityCmd(IConsoleShell shell, string argstr, string[] args)
    {
        if (!EntityUid.TryParse(args[0], out var uid))
        {
            shell.WriteError("Couldn't parse entity.");
            return;
        }

        var unshielded = UnshieldEntity(uid);

        if (unshielded)
            shell.WriteLine("Removed shield from " + uid);
        else
            shell.WriteError("No shield to remove from " + uid);
    }
}
