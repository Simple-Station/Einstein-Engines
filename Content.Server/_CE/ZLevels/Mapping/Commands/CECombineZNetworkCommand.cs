/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Server._CE.ZLevels.Core;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.Map;

namespace Content.Server._CE.ZLevels.Mapping.Commands;

[AdminCommand(AdminFlags.Server | AdminFlags.Mapping)]
public sealed class CECombineZNetworkCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly CEZLevelsSystem _zLevels = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

    public override string Command => "znetwork-combine";
    public override string Description => "Connects a number of maps into a common network of z-levels. Does not work if one of the maps is already in the z-level network";

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return CompletionResult.FromHintOptions(CompletionHelper.MapIds(_entities), "Map Id in order from ground to sky");
    }

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length == 1)
        {
            shell.WriteError("Not enough maps to form a network of levels");
            return;
        }

        List<MapId> maps = new();
        foreach (var arg in args)
        {
            if (!int.TryParse(arg, out var mapIdInt))
            {
                shell.WriteError($"Cannot parse `{arg}` into mapId");
                return;
            }

            var mapId = new MapId(mapIdInt);

            if (mapId == MapId.Nullspace)
            {
                shell.WriteError($"Cannot parse NullSpace");
                return;
            }

            if (!_map.MapExists(mapId))
            {
                shell.WriteError($"Map {mapId} dont exist");
                return;
            }

            if (maps.Contains(mapId))
            {
                shell.WriteError($"Duplication maps: {mapId}");
                return;
            }

            maps.Add(mapId);
        }

        var network = _zLevels.CreateZNetwork();
        _meta.SetEntityName(network, $"Combined zNetwork: {network.Owner.Id}");
        var counter = 0;
        Dictionary<EntityUid, int> dict = new();
        foreach (var findMap in maps)
        {
            dict.Add( _map.GetMap(findMap), counter);
            counter++;
        }

        var success = _zLevels.TryAddMapsIntoZNetwork(network, dict);

        if (success)
            shell.WriteLine($"Created z-level network! Z-Network entity: {network}");
        else
            shell.WriteLine($"Created z-level network {network}, but something went wrong!");
    }
}
