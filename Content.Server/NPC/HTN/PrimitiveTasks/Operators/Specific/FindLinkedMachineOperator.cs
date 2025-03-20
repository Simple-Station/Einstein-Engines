using System.Threading;
using System.Threading.Tasks;
using Content.Server.NPC.Pathfinding;
using Content.Shared.DeviceLinking;
using Content.Shared.Interaction;
using Content.Shared.Silicons.Bots;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class FindLinkedMachineOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private PathfindingSystem _pathfinding = default!;

    [DataField] public string RangeKey = NPCBlackboard.FillbotPickupRange;

    /// <summary>
    /// Target entity to find
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
        _pathfinding = sysManager.GetEntitySystem<PathfindingSystem>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<float>(RangeKey, out var _, _entManager)
            || !_entManager.TryGetComponent<FillbotComponent>(owner, out var fillbot)
            || !_entManager.TryGetComponent<DeviceLinkSourceComponent>(owner, out var fillbotlinks)
            || fillbotlinks.LinkedPorts.Count != 1
            || fillbot.LinkedSinkEntity == null
            || _entManager.Deleted(fillbot.LinkedSinkEntity))
            return (false, null);

        var path = await _pathfinding.GetPath(owner, fillbot.LinkedSinkEntity.Value, SharedInteractionSystem.InteractionRange - 0.5f, cancelToken);

        if (path.Result == PathResult.NoPath)
            return (false, null);

        return (true, new()
        {
            {TargetKey, fillbot.LinkedSinkEntity},
            {TargetMoveKey, _entManager.GetComponent<TransformComponent>(fillbot.LinkedSinkEntity.Value).Coordinates},
            {NPCBlackboard.PathfindKey, path},
        });
    }
}
