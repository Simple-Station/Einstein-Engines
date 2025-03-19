using System.Threading;
using System.Threading.Tasks;
using Content.Server.Disposal.Unit.Components;
using Content.Server.Item;
using Content.Server.NPC.Pathfinding;
using Content.Shared.Body.Part;
using Content.Shared.DeviceLinking;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Materials;
using Content.Shared.Silicons.Bots;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;


namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class PickNearbyFillableItemOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    private SharedMaterialStorageSystem _sharedMaterialStorage = default!;
    private EntityLookupSystem _lookup = default!;
    private PathfindingSystem _pathfinding = default!;
    private SharedHandsSystem _sharedHandsSystem = default!;
    private TagSystem _tagSystem = default!;

    private const string TrashTagKey = "Trash";

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
        _sharedMaterialStorage = sysManager.GetEntitySystem<SharedMaterialStorageSystem>();
        _sharedHandsSystem = sysManager.GetEntitySystem<SharedHandsSystem>();
        _tagSystem = sysManager.GetEntitySystem<TagSystem>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<float>(RangeKey, out var range, _entManager)
            || !_entManager.TryGetComponent<FillbotComponent>(owner, out var fillbot)
            || !_entManager.TryGetComponent<DeviceLinkSourceComponent>(owner, out var fillbotlinks)
            || fillbotlinks.LinkedPorts.Count != 1
            || fillbot.LinkedSinkEntity == null)
            return (false, null);

        var isMaterialStorage = _entManager.TryGetComponent<MaterialStorageComponent>(
                fillbot.LinkedSinkEntity,
                out var linkedStorage);

        var isDisposalUnit = _entManager.TryGetComponent<DisposalUnitComponent>(
            fillbot.LinkedSinkEntity,
            out var disposalUnit);

        foreach (var target in _lookup.GetEntitiesInRange(owner, range))
        {
            // only things the robot can actually pick up
            if (!_sharedHandsSystem.CanPickupAnyHand(owner, target))
                continue;

            // only things not currently contained by something else
            if (_entManager.TryGetComponent<MetaDataComponent>(target, out var meta) &&
                meta.Flags.HasFlag(MetaDataFlags.InContainer))
                continue;

            // only things that can go inside
            if (linkedStorage != null && !_sharedMaterialStorage.CanInsertMaterialEntity(target, fillbot.LinkedSinkEntity.Value))
                continue;

            // trash only
            if (disposalUnit != null &&
                (_whitelistSystem.IsWhitelistFail(disposalUnit.Whitelist, target)
                    || !_tagSystem.HasTag(target, _prototypeManager.Index<TagPrototype>(TrashTagKey))
                    || _entManager.HasComponent<BodyPartComponent>(target))) // Robot is unable to insert bodyparts into Disposals for some reason
                continue;

            const float pathRange = SharedInteractionSystem.InteractionRange - 1;
            var path = await _pathfinding.GetPath(owner, target, pathRange, cancelToken);

            if (path.Result == PathResult.NoPath)
                continue;

            return (true, new()
            {
                {TargetKey, target},
                {TargetMoveKey, _entManager.GetComponent<TransformComponent>(target).Coordinates},
                {NPCBlackboard.PathfindKey, path},
            });
        }

        return (false, null);
    }
}
