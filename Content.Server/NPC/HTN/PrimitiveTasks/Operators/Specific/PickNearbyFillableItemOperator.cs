using System.Threading;
using System.Threading.Tasks;
using Content.Server.Disposal.Unit.Components;
using Content.Server.NPC.Pathfinding;
using Content.Shared.DeviceLinking;
using Content.Shared.Interaction;
using Content.Shared.Materials;
using Content.Shared.Silicons.Bots;
using Content.Shared.Tag;
using Content.Shared.Whitelist;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class PickNearbyFillableItemOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    private EntityLookupSystem _lookup = default!;
    private PathfindingSystem _pathfinding = default!;
    private TagSystem _tagSystem = default!;

    [DataField] public string RangeKey = NPCBlackboard.FillbotPickupRange;

    /// <summary>
    /// Target entity to pick up
    /// </summary>
    [DataField(required: true)]
    public string TargetKey = string.Empty;

    /// <summary>
    /// Target entitycoordinates to move to.
    /// </summary>
    [DataField(required: true)]
    public string TargetMoveKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _lookup = sysManager.GetEntitySystem<EntityLookupSystem>();
        _pathfinding = sysManager.GetEntitySystem<PathfindingSystem>();
        _tagSystem = sysManager.GetEntitySystem<TagSystem>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<float>(RangeKey, out var range, _entManager)
            || !_entManager.TryGetComponent<FillbotComponent>(owner, out var fillbot)
            || !_entManager.TryGetComponent<DeviceLinkSourceComponent>(owner, out var fillbotlinks)
            || fillbotlinks.LinkedPorts.Count != 1)
            return (false, null);

        var isMaterialStorage = _entManager.TryGetComponent<MaterialStorageComponent>(
                fillbot.LinkedSinkEntity,
                out var linkedStorage);

        var isDisposalUnit = _entManager.TryGetComponent<DisposalUnitComponent>(
            fillbot.LinkedSinkEntity,
            out var disposalUnit);

        foreach (var target in _lookup.GetEntitiesInRange(owner, range))
        {
            if (linkedStorage != null && _whitelistSystem.IsWhitelistFail(linkedStorage.Whitelist, target))
                continue;

            if(disposalUnit != null && _whitelistSystem.IsWhitelistFail(disposalUnit.Whitelist, target))
                continue;

            var pathRange = SharedInteractionSystem.InteractionRange;

            //Needed to make sure it doesn't sometimes stop right outside its interaction range, in case of a mob.
            pathRange -= 0.5f;

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

        return (false, null);
    }
}
