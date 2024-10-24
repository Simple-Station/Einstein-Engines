using Content.Shared.Damage.Components;
using Content.Shared.Inventory;

namespace Content.Shared.Damage.Events;

/// <summary>
/// The components in the list are going to be hit,
/// give opportunities to change the damage or other stuff.
/// </summary>
public sealed class TakeStaminaDamageEvent : HandledEntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = ~SlotFlags.POCKET;

    /// <summary>
    /// List of hit stamina components.
    /// </summary>
    public EntityUid Target;

    /// <summary>
    /// The multiplier. Generally, try to use *= or /= instead of overwriting.
    /// </summary>
    public float Multiplier = 1;

    /// <summary>
    /// The flat modifier. Generally, try to use += or -= instead of overwriting.
    /// </summary>
    public float FlatModifier = 0;

    public TakeStaminaDamageEvent(EntityUid target)
    {
        Target = target;
    }
}
