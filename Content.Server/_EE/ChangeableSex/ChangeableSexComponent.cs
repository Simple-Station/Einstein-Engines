using Content.Shared.Humanoid;
using Robust.Shared.Enums;

namespace Content.Server._EE.ChangeableSex;

/// <summary>
/// Adds a verb to select an entity's sex.
/// </summary>
[RegisterComponent]
public sealed partial class ChangeableSexComponent : Component
{
    /// <summary>
    /// Whether the entity's gender can only be renamed once.
    /// If set to true, the component will be removed after selecting a gender.
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
