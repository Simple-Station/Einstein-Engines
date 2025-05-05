using Content.Shared.Humanoid;

namespace Content.Server._EE.ChangeableSex;

/// <summary>
/// Adds a verb to select an entity's sex.
/// </summary>
[RegisterComponent]
public sealed partial class ChangeableSexComponent : Component
{
    /// <summary>
    /// Whether the entity's sex can only be changed once.
    /// If set to true, the component will be removed after selecting a sex.
    /// </summary>
    [DataField]
    public bool SingleUse = false;

    [DataField("sexList")]
    public Dictionary<string, Sex> SexList = new()
        {
            { "Male", Sex.Male },
            { "Female", Sex.Female },
            { "Unsexed", Sex.Unsexed }
        };
}
