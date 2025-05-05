using Robust.Shared.Enums;

namespace Content.Server._EE.ChangeableGender;

/// <summary>
/// Adds a verb to select an entity's pronouns.
/// </summary>
[RegisterComponent]
public sealed partial class ChangeableGenderComponent : Component
{
    /// <summary>
    /// Whether the entity's gender can only be changed once.
    /// If set to true, the component will be removed after selecting a gender.
    /// </summary>
    [DataField]
    public bool SingleUse = false;

    [DataField("genderList")]
    public Dictionary<string, Gender> GenderList = new()
        {
            { "He/Him", Gender.Male },
            { "She/Her", Gender.Female },
            { "They/Them", Gender.Epicene },
            { "It/Its", Gender.Neuter }
        };
}
