using Robust.Shared.GameStates;

namespace Content.Shared.WhiteDream.BloodCult.Components;

[Virtual, RegisterComponent, NetworkedComponent]
public sealed partial class CultItemComponent : Component
{
    /// <summary>
    ///     Allow non-cultists to use this item?
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool AllowUseToEveryone;
}
