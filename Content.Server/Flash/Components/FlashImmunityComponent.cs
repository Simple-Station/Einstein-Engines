using Robust.Shared.GameStates;

namespace Content.Server.Flash.Components;

/// <summary>
///     Makes the entity immune to being flashed.
///     When given to clothes in the "head", "eyes" or "mask" slot it protects the wearer.
/// </summary>
[RegisterComponent] // Goob edit
public sealed partial class FlashImmunityComponent : Component
{
    [DataField]
    public bool Enabled { get; set; } = true;
}
