using Content.Shared.DoAfter;

namespace Content.Server.Resist;

[RegisterComponent]
public sealed partial class CanEscapeInventoryComponent : Component
{
    /// <summary>
    ///     Base doafter length for uncontested breakouts.
    /// </summary>
    [DataField]
    public float BaseResistTime = 5f;

    public bool IsEscaping => DoAfter != null;

    [DataField("doAfter")]
    public DoAfterId? DoAfter;

    /// <summary>
    ///     Action to cancel inventory escape.
    /// </summary>
    [DataField]
    public EntityUid? EscapeCancelAction;
}
