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
using Content.Shared.Tag;
using Robust.Shared.Prototypes;


namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class PickNearbyWeldableOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private EntityLookupSystem _lookup = default!;
    private WeldbotSystem _weldbot = default!;
    private PathfindingSystem _pathfinding = default!;
    private TagSystem _tagSystem = default!;

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
        _tagSystem = sysManager.GetEntitySystem<TagSystem>();
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
        bool emagged = _entManager.HasComponent<EmaggedComponent>(owner);

        foreach (var target in _lookup.GetEntitiesInRange(owner, range))
        {
            if (damageQuery.TryGetComponent(target, out var damage))
            {
                var tagPrototype = _prototypeManager.Index<TagPrototype>("SiliconMob");

                if (!_entManager.TryGetComponent<TagComponent>(target, out var tagComponent) || !_tagSystem.HasTag(tagComponent, tagPrototype))
                    continue;

                // Only go towards a target if the bot can actually help them or if the weldbot is emagged
                if (!emagged && damage.DamagePerGroup["Brute"].Value == 0)
                    continue;

                //Needed to make sure it doesn't sometimes stop right outside it's interaction range
                var pathRange = SharedInteractionSystem.InteractionRange - 1f;
                var path = await _pathfinding.GetPath(owner, target, pathRange, cancelToken);

                if (path.Result == PathResult.NoPath)
                    continue;

                return (true, new Dictionary<string, object>()
                {
                    {TargetKey, target},
                    {TargetMoveKey, _entManager.GetComponent<TransformComponent>(target).Coordinates},
                    {NPCBlackboard.PathfindKey, path},
                });
            }
        }

        return (false, null);
    }
}
