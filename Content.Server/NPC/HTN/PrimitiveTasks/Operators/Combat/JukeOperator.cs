using Content.Server.NPC.Components;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Combat;

public sealed partial class JukeOperator : HTNOperator, IHtnConditionalShutdown
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField]
    public JukeType JukeType = JukeType.AdjacentTile;

    [DataField]
    public HTNPlanState ShutdownState { get; private set; } = HTNPlanState.PlanFinished;

    /// <summary>
    ///     Controls how long(in seconds) the NPC will move while juking.
    /// </summary>
    [DataField]
    public float JukeDuration = 0.5f;

    /// <summary>
    ///     Controls how often (in seconds) an NPC will try to juke.
    /// </summary>
    [DataField]
    public float JukeCooldown = 3f;

    public override void Startup(NPCBlackboard blackboard)
    {
        base.Startup(blackboard);
        var juke = _entManager.EnsureComponent<NPCJukeComponent>(blackboard.GetValue<EntityUid>(NPCBlackboard.Owner));
        juke.JukeType = JukeType;
        juke.JukeDuration = JukeDuration;
        juke.JukeCooldown = JukeCooldown;
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        return HTNOperatorStatus.Finished;
    }

    public void ConditionalShutdown(NPCBlackboard blackboard)
    {
        _entManager.RemoveComponent<NPCJukeComponent>(blackboard.GetValue<EntityUid>(NPCBlackboard.Owner));
    }
}
