/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Linq;
using Content.Server._CE.ZLevels.Core;
using Content.Server.Administration;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Server._CE.ZLevels.Mapping.Commands;

[AdminCommand(AdminFlags.Server | AdminFlags.Mapping)]
public sealed class CEAddMapAboveZNetworkCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly IResourceManager _resourceMgr = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly CEZLevelsSystem _zLevel = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

    public override string Command => "znetwork-add-above";
    public override string Description => "Add a map above an existing z-network.";

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        switch (args.Length)
        {
            case 1:
                var options = new List<CompletionOption>();
                var query = _entities.EntityQueryEnumerator<CEZLevelsNetworkComponent, MetaDataComponent>();
                while (query.MoveNext(out var uid, out var zLevelComp, out var meta))
                {
                    options.Add(new CompletionOption(_entities.GetNetEntity(uid).ToString(), meta.EntityName));
                }
                return CompletionResult.FromHintOptions(options, "zNetwork net entity");
            case 2:
                var opts = CompletionHelper.UserFilePath(args[1], _resourceMgr.UserData)
                    .Concat(CompletionHelper.ContentFilePath(args[1], _resourceMgr));
                return CompletionResult.FromHintOptions(opts, Loc.GetString("cmd-hint-mapping-path"));
        }
        return CompletionResult.Empty;
    }

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player)
        {
            shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        // Get the target network
        EntityUid? target;

        if (!NetEntity.TryParse(args[0], out var targetNet) ||
            !_entities.TryGetEntity(targetNet, out target))
        {
            shell.WriteError($"Unable to find entity {args[0]}");
            return;
        }

        if (!_entities.TryGetComponent<CEZLevelsNetworkComponent>(target, out var levelComp))
        {
            shell.WriteError($"Target entity doesn't have CEZLevelsNetworkComponent {args[0]}");
            return;
        }

        // Load the map
        var path = new ResPath(args[1]);
        var opts = new DeserializationOptions { StoreYamlUids = true };

        if (!_mapLoader.TryLoadMap(path, out var mapEnt, out _, opts))
        {
            shell.WriteError($"Failed to load map: {path.ToString()}!");
            return;
        }

        if (!_entities.TryGetComponent<MapComponent>(mapEnt.Value, out var mapComp))
        {
            shell.WriteError($"Loaded entity {mapEnt.Value} doesn't have MapComponent.");
            _entities.QueueDeleteEntity(mapEnt.Value);
            return;
        }

        // Calculate the depth (one level above the maximum existing depth)
        var maxDepth = levelComp.ZLevels.Count > 0
            ? levelComp.ZLevels.Keys.Max()
            : 0;
        var newDepth = maxDepth + 1;

        // Add the map to the network
        var dict = new Dictionary<EntityUid, int> { { mapEnt.Value, newDepth } };

        if (!_zLevel.TryAddMapsIntoZNetwork((target.Value, levelComp), dict))
        {
            shell.WriteError($"Failed to add map to z-network at depth {newDepth}.");
            _entities.QueueDeleteEntity(mapEnt.Value);
            return;
        }

        _meta.SetEntityName(mapEnt.Value, $"{path.FilenameWithoutExtension} [{newDepth}]");

        shell.WriteLine($"Successfully added map {path.FilenameWithoutExtension} to z-network at depth {newDepth}.");
        shell.WriteLine($"Map ID: {mapComp.MapId}");
    }
}
