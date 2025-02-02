using System.Threading;
using System.Threading.Tasks;
using Content.Server.Atmos.Components;
using Content.Server.NPC.Pathfinding;
using Content.Server.Repairable;
using Content.Shared.Damage;
using Content.Shared.Emag.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Silicon.Components;
using Content.Shared.Silicons.Bots;


namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class PickNearbyWeldableOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private EntityLookupSystem _lookup = default!;
    private WeldbotSystem _weldbot = default!;
    private PathfindingSystem _pathfinding = default!;

    [DataField("rangeKey")] public string RangeKey = NPCBlackboard.WeldbotWeldRange;

    /// <summary>
    /// Target entity to weld
    /// </summary>
    [DataField("targetKey", required: true)]
    public string TargetKey = string.Empty;

    /// <summary>
    /// Target entitycoordinates to move to.
    /// </summary>
    [DataField("targetMoveKey", required: true)]
    public string TargetMoveKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _lookup = sysManager.GetEntitySystem<EntityLookupSystem>();
        _weldbot = sysManager.GetEntitySystem<WeldbotSystem>();
        _pathfinding = sysManager.GetEntitySystem<PathfindingSystem>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<float>(RangeKey, out var range, _entManager))
            return (false, null);

        if (!_entManager.TryGetComponent<WeldbotComponent>(owner, out var weldbot))
            return (false, null);

        var damageQuery = _entManager.GetEntityQuery<DamageableComponent>();
        var injectQuery = _entManager.GetEntityQuery<SiliconComponent>();
        var mobState = _entManager.GetEntityQuery<MobStateComponent>();
        var emaggedQuery = _entManager.GetEntityQuery<EmaggedComponent>();

        foreach (var entity in _lookup.GetEntitiesInRange(owner, range))
        {
            if (mobState.TryGetComponent(entity, out var state) &&
                injectQuery.HasComponent(entity) &&
                damageQuery.TryGetComponent(entity, out var damage))
            {
                if (_entManager.TryGetComponent<RepairableComponent>(entity, out var repairableComponent))
                    continue;

                bool emagged = emaggedQuery.HasComponent(entity);

                // Only go towards a target if the bot can actually help them or if the weldbot is emagged
                if (!emagged && damage.DamagePerGroup["Brute"].Value > 0)
                    continue;

                // Ignore non-flammable components if the bot is emagged
                if (emagged && !_entManager.HasComponent<FlammableComponent>(entity))
                    continue;

                //Needed to make sure it doesn't sometimes stop right outside it's interaction range
                var pathRange = SharedInteractionSystem.InteractionRange - 1f;
                var path = await _pathfinding.GetPath(owner, entity, pathRange, cancelToken);

                if (path.Result == PathResult.NoPath)
                    continue;

                return (true, new Dictionary<string, object>()
                {
                    {TargetKey, entity},
                    {TargetMoveKey, _entManager.GetComponent<TransformComponent>(entity).Coordinates},
                    {NPCBlackboard.PathfindKey, path},
                });
            }
        }

        return (false, null);
    }
}
