using Content.Server.DeviceLinking.Components;
using Content.Server.NPC;
using Content.Server.NPC.HTN.Preconditions;


public sealed partial class SignalControlPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField("isOnIfNoSignalControl")]
    public bool IsOnIfNoSignalControl = false;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        if (blackboard.TryGetValue<EntityUid>(NPCBlackboard.Owner, out var owner, _entManager))
        {
            if (_entManager.TryGetComponent<SignalControlComponent>(owner, out var control))
            {
                return control.IsOn;
            }

            return IsOnIfNoSignalControl;
        }

        return false;
    }
}
