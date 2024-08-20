using Content.Server.Psionics.NPC.GlimmerWisp;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class DrainOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private GlimmerWispSystem _wispSystem = default!;

    [DataField("drainKey")]
    public string DrainKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _wispSystem = sysManager.GetEntitySystem<GlimmerWispSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var target = blackboard.GetValue<EntityUid>(DrainKey);

        if (!target.IsValid() || _entManager.Deleted(target))
            return HTNOperatorStatus.Failed;

        if (!_entManager.TryGetComponent<GlimmerWispComponent>(owner, out var wisp))
            return HTNOperatorStatus.Failed;

        if (wisp.IsDraining)
            return HTNOperatorStatus.Continuing;

        if (wisp.DrainTarget == null)
        {
            if (_wispSystem.NPCStartLifedrain(owner, target, wisp))
                return HTNOperatorStatus.Continuing;
            else
                return HTNOperatorStatus.Failed;
        }

        wisp.DrainTarget = null;
        return HTNOperatorStatus.Finished;
    }
}
