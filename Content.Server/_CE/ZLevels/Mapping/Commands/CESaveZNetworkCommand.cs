/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Server.Administration;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared.Administration;
using Robust.Server.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Server._CE.ZLevels.Mapping.Commands;

[AdminCommand(AdminFlags.Server | AdminFlags.Mapping)]
public sealed class CESaveZNetworkCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;

    public override string Command => "znetwork-save";
    public override string Description => "Save all zNetwork maps to default server folder";

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var options = new List<CompletionOption>();
            var query = _entities.EntityQueryEnumerator<CEZLevelsNetworkComponent, MetaDataComponent>();
            while (query.MoveNext(out var uid, out _, out var meta))
            {
                options.Add(new CompletionOption(_entities.GetNetEntity(uid).ToString(), meta.EntityName));
            }
            return CompletionResult.FromHintOptions(options, "zNetwork net entity");
        }
        if (args.Length == 2)
        {
            return CompletionResult.FromHint("ZNetwork name (for example: `Dev`)");
        }
        return CompletionResult.Empty;
    }

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError("Wrong arguments count.");
            return;
        }

        // get the target
        EntityUid? target;

        if (!NetEntity.TryParse(args[0], out var targetNet) ||
            !_entities.TryGetEntity(targetNet, out target))
        {
            shell.WriteError($"Unable to find entity {args[1]}");
            return;
        }

        if (!_entities.TryGetComponent<CEZLevelsNetworkComponent>(target, out var levelComp))
        {
            shell.WriteError($"Target entity doesnt have CEZLevelsNetworkComponent {args[1]}");
            return;
        }

        foreach (var (depth, mapUid) in levelComp.ZLevels)
        {
            if (!_entities.TryGetComponent<MapComponent>(mapUid, out var mapComp))
            {
                shell.WriteError($"Map entity {mapUid} doesnt have MapComponent.");
                continue;
            }

            var mapId = mapComp.MapId;

            // no saving null space
            if (mapId == MapId.Nullspace)
                return;

            if (!_map.MapExists(mapId))
            {
                shell.WriteError($"Map {mapId} doesnt exist!");
                return;
            }

            if (_map.IsInitialized(mapId))
            {
                shell.WriteError($"Map {mapId} is already initialized, cannot save initialized maps!");
                return;
            }

            var savePath = new ResPath($"/ZNetworkSaves/{args[1]}/{args[1]}{depth}.yml");
            shell.WriteLine(Loc.GetString("cmd-savemap-attempt", ("mapId", mapId), ("path", savePath)));
            if (_mapLoader.TrySaveMap(mapId, savePath))
            {
                shell.WriteLine(Loc.GetString("cmd-savemap-success"));
            }
            else
            {
                shell.WriteError(Loc.GetString("cmd-savemap-error"));
            }
        }
    }
}
