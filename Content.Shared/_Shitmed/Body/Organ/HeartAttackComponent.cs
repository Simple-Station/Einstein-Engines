namespace Content.Shared._Shitmed.Body.Organ;

// I wanted to name this SheerHeartAttackComponent :(
[RegisterComponent]
public sealed partial class HeartAttackComponent : Component
{

    /// <summary>
    ///     Movement speed modifier for walking.
    /// </summary>
    [DataField]
    public float WalkSpeed = 1f;

    /// <summary>
    ///     Movement speed modifier for sprinting.
    /// </summary>
    [DataField]
    public float SprintSpeed = 1f;
}