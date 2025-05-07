using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing;

[RegisterComponent, NetworkedComponent]
public sealed partial class MagbootsComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> MagbootsAlert = "Magboots";

    /// <summary>
    /// If true, the user must be standing on a grid or planet map to experience the weightlessness-canceling effect
    /// </summary>
    [DataField]
    public bool RequiresGrid = true;

    /// <summary>
    /// Slot the clothing has to be worn in to work.
    /// </summary>
    [DataField]
    public string Slot = "shoes";

    /// <summary>
    ///     Whether or not activating the magboots changes a sprite.
    /// </summary>
    [DataField]
    public bool ChangeClothingVisuals;

    /// <summary>
    ///     Whether or not the magboots are currently Active.
    /// </summary>
    [DataField]
    public bool Active;

    /// <summary>
    ///     Walk speed modifier to use while the magnets are active.
    /// </summary>
    [DataField]
    public float ActiveWalkModifier = 0.85f;

    /// <summary>
    ///     Sprint speed modifier to use while the magnets are active.
    /// </summary>
    [DataField]
    public float ActiveSprintModifier = 0.80f;

    /// <summary>
    ///     Walk speed modifier to use while the magnets are off.
    /// </summary>
    [DataField]
    public float InactiveWalkModifier = 1f;

    /// <summary>
    ///     Sprint speed modifier to use while the magnets are off.
    /// </summary>
    [DataField]
    public float InactiveSprintModifier = 1f;
}
