using Content.Shared.Inventory;

namespace Content.Shared.Movement.Events;

/// <summary>
/// Raised on an entity to check if it has a max contact slowdown.
/// </summary>
[ByRefEvent]
public record struct GetSpeedModifierContactCapEvent() : IInventoryRelayEvent
{
    SlotFlags IInventoryRelayEvent.TargetSlots => ~SlotFlags.POCKET;

    public float WalkSpeedModifier;

    public float SprintSpeedModifier;

    public void SetIfMax(float walkSpeedModifier, float sprintSpeedModifier)
    {
        WalkSpeedModifier = MathF.Max(WalkSpeedModifier, walkSpeedModifier);
        SprintSpeedModifier = MathF.Max(SprintSpeedModifier, sprintSpeedModifier);
    }

    public GetSpeedModifierContactCapEvent(float walkSpeedModifier, float sprintSpeedModifier) : this()
    {
        WalkSpeedModifier = walkSpeedModifier;
        SprintSpeedModifier = sprintSpeedModifier;
    }
}
