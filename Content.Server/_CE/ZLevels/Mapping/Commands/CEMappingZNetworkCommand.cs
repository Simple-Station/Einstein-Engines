/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Server._CE.ZLevels.Core;
using Content.Server.Administration;
using Content.Server.GameTicking;
using Content.Shared._CE.ZLevels.Mapping.Prototypes;
using Content.Shared.Administration;
using Robust.Server.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._CE.ZLevels.Mapping.Commands;

[AdminCommand(AdminFlags.Server | AdminFlags.Mapping)]
public sealed class CEMappingZNetworkCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly CEZLevelsSystem _zLevel = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly MapSystem _map = default!;

    public override string Command => "znetwork-mapping";
    public override string Description => "Load CEZLevelMapPrototype as ZNetwork for mapping";


    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        var options = new List<CompletionOption>();
        foreach (var map in _proto.EnumeratePrototypes<CEZLevelMapPrototype>())
        {
            options.Add(new CompletionOption(map.ID));
        }

        return CompletionResult.FromHintOptions(options, "CEZLevelMapPrototype");
    }

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player)
        {
            shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        if (args.Length != 1)
        {
            shell.WriteError("Wrong arguments count.");
            return;
        }
        //Get Map Prototype
        if (!_proto.Resolve<CEZLevelMapPrototype>(args[0], out var indexedZMap))
        {
            shell.WriteError($"Unknown CEZLevelMapPrototype {args[0]}");
            return;
        }

        //Ok all parsing is done, we start creating maps

        var network = _zLevel.CreateZNetwork(indexedZMap.Components);
        _meta.SetEntityName(network, $"Mapping zNetwork: {indexedZMap.ID}");
        Dictionary<EntityUid, int> dict = new();

        List<MapId> createdMaps = new();

        var opts = new DeserializationOptions {StoreYamlUids = true};

        //Loading maps
        var depth = 0;
        foreach (var path in indexedZMap.Maps)
        {
            if (!_mapLoader.TryLoadMap(path, out var mapEnt, out _, opts))
            {
                shell.WriteError($"Failed to load zNetwork map (depth {depth}): {path.ToString()}!");
                return;
            }

            dict.Add(mapEnt.Value, depth);
            createdMaps.Add(mapEnt.Value.Comp.MapId);
            _meta.SetEntityName(mapEnt.Value, $"Mapping {indexedZMap.ID} [{depth}]");
            depth++;
        }

        //Was the maps actually created or did it fail somehow?
        var success = true;
        foreach (var mapId in createdMaps)
        {
            if (!_map.MapExists(mapId))
            {
                success = false;
                shell.WriteError($"For some reason some maps dont exist after loading! MapId: {mapId}");
            }
        }

        if (!_zLevel.TryAddMapsIntoZNetwork(network, dict))
        {
            shell.WriteError($"Failed to create zNetwork from loaded maps!");
            return;
        }

        if (!success)
        {
            shell.WriteError("Unloading all created maps...");
            foreach (var mapId in createdMaps)
            {
                _map.DeleteMap(mapId);
            }
            EntityManager.QueueDeleteEntity(network);
            return;
        }

        //Maps successfully created. run misc helpful mapping commands
        if (player.AttachedEntity is { Valid: true } playerEntity &&
            (EntityManager.GetComponent<MetaDataComponent>(playerEntity).EntityPrototype is not { } proto || proto.ID != GameTicker.AdminObserverPrototypeName))
        {
            shell.ExecuteCommand("aghost");
        }

        // don't interrupt mapping with events or auto-shuttle
        shell.ExecuteCommand("changecvar events.enabled false");
        shell.ExecuteCommand("changecvar shuttle.auto_call_time 0");

        //TODO: Autosaves

        shell.ExecuteCommand($"tp 0 0 {createdMaps[0]}");
        shell.RemoteExecuteCommand("mappingclientsidesetup");
        foreach (var mapId in createdMaps)
        {
            DebugTools.Assert(_map.IsPaused(mapId));
        }
    }
}
