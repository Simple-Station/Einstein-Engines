using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.Roles;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;


namespace Content.Server.Administration.Commands;


[AdminCommand(AdminFlags.Admin)]
public sealed class SetStationAiNameCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private readonly ProtoId<JobPrototype> _stationAiJob = "StationAi";

    public string Command => "setstationainame";
    public string Description => Loc.GetString("set-station-ai-name-command-description");
    public string Help => Loc.GetString("set-station-ai-name-command-help-text", ("command", Command));

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!int.TryParse(args[0], out var entInt))
        {
            shell.WriteLine(Loc.GetString("shell-entity-uid-must-be-number"));
            return;
        }

        var netEntity = new NetEntity(entInt);

        if (!_entManager.TryGetEntity(netEntity, out var target))
        {
            shell.WriteLine(Loc.GetString("shell-invalid-entity-id"));
            return;
        }

        var hasStationAi = _prototypeManager.TryIndex(_stationAiJob, out var job);

        if (!hasStationAi)
        {
            shell.WriteLine(Loc.GetString("set-station-ai-name-command-no-station-ai"));
            return;
        }

        var spawningSystem = _entManager.System<StationSpawningSystem>();
        spawningSystem.EquipJobName(target.Value, job!);
        shell.WriteLine(Loc.GetString("shell-command-success"));
    }
}
