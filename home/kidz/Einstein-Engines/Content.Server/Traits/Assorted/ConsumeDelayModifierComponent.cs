using Robust.Shared.GameStates;

namespace Content.Server.Traits.Assorted.Components;

/// <summary>
///     This is used for any trait that modifies how fast an entity consumes food and drinks.
/// </summary>
[RegisterComponent]
public sealed partial class ConsumeDelayModifierComponent : Component
{
    /// <summary>
    ///     What to multiply the eating delay by.
    /// </summary>
    [DataField]
    public float FoodDelayMultiplier { get; set; } = 1f;

    /// <summary>
    ///     What to multiply the drinking delay by.
    /// </summary>
    [DataField]
    public float DrinkDelayMultiplier { get; set; } = 1f;
}
