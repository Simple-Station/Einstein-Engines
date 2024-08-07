namespace Content.Server.Traits.Assorted;

/// <summary>
///  This component is used for traits that modify movement speed.
/// </summary>
[RegisterComponent]
public sealed partial class TraitSpeedModifierComponent : Component
{
    [DataField(required: true)]
    public float WalkModifier = 1.0f;

    [DataField("sprintModifier", required: true)]
    public float SprintModifier = 1.0f;
}
