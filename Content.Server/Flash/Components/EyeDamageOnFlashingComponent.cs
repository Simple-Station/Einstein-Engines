namespace Content.Server.Flash.Components;

[RegisterComponent]
public sealed partial class EyeDamageOnFlashingComponent : Component
{
    /// <summary>
    ///   How much to modify the duration of flashes against this entity.
    /// </summary>
    [DataField]
    public float FlashDurationMultiplier = 1.5f;

    /// <summary>
    ///   Chance to get EyeDamage on flash
    /// </summary>
    [DataField]
    public float EyeDamageChance = 0.3f;

    /// <summary>
    ///   How many EyeDamage when flashed? (If EyeDamageChance check passed)
    /// </summary>
    [DataField]
    public int EyeDamage = 1;
}
