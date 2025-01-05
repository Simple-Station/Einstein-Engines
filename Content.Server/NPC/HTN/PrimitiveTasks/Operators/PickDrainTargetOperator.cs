using System.Threading;
using System.Threading.Tasks;
using Content.Server.LifeDrainer;
using Content.Server.NPC.Pathfinding;
using Content.Server.NPC.Systems;
using NpcFactionSystem = Content.Shared.NPC.Systems.NpcFactionSystem;


namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class PickDrainTargetOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entMan = default!;

    private LifeDrainerSystem _drainer = default!;
    private NpcFactionSystem _faction = default!;
    private PathfindingSystem _pathfinding = default!;

    private EntityQuery<LifeDrainerComponent> _drainerQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    [DataField(required: true)] public string
        RangeKey = string.Empty,
        TargetKey = string.Empty,
        DrainKey = string.Empty;

    /// <summary>
    /// Where the pathfinding result will be stored (if applicable). This gets removed after execution.
    /// </summary>
    [DataField]
    public string PathfindKey = NPCBlackboard.PathfindKey;

    public override void Initialize(IEntitySystemManager sysMan)
    {
        base.Initialize(sysMan);

        _drainer = sysMan.GetEntitySystem<LifeDrainerSystem>();
        _faction = sysMan.GetEntitySystem<NpcFactionSystem>();
        _pathfinding = sysMan.GetEntitySystem<PathfindingSystem>();

        _drainerQuery = _entMan.GetEntityQuery<LifeDrainerComponent>();
        _xformQuery = _entMan.GetEntityQuery<TransformComponent>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard, CancellationToken cancelToken) {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        if (!_drainerQuery.TryComp(owner, out var drainer))
            return (false, null);

        var ent = (owner, drainer);
        if (!blackboard.TryGetValue<float>(RangeKey, out var range, _entMan))
            return (false, null);

        // find crit psionics nearby
        foreach (var target in _faction.GetNearbyHostiles(owner, range))
        {
            if (!_drainer.CanDrain(ent, target) || !_xformQuery.TryComp(target, out var xform))
                continue;

            // pathfind to the first crit psionic in range to start draining
            var targetCoords = xform.Coordinates;
            var path = await _pathfinding.GetPath(owner, target, range, cancelToken);
            if (path.Result != PathResult.Path)
                continue;

            return (true, new Dictionary<string, object>()
            {
                { TargetKey, targetCoords },
                { DrainKey, target },
                { PathfindKey, path }
            });
        }

        return (false, null);
    }
}
