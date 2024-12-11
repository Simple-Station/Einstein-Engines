using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.Components;

/// <summary>
///   Tries to toggle clothing after spawning with equipment.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ToggleStartingGearComponent : Component
{
    /// <summary>
    ///     The inventory slot to toggle.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public string Slot;
}
