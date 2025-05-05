namespace Content.Server._EE.ChangableGender;

/// <summary>
/// Adds a verb to select an entity's pronouns.
/// </summary>
[RegisterComponent]
public sealed partial class ChangableGenderComponent : Component
{
    /// <summary>
    /// Whether the entity's gender can only be renamed once.
    /// If set to true, the component will be removed after selecting a gender.
    /// </summary>
    [DataField]
    public bool SingleUse = false;

    [DataField("genderList")]
    public Dictionary<string, Robust.Shared.Enums.Gender> GenderList = new()
        {
            { "He/Him", Robust.Shared.Enums.Gender.Male },
            { "She/Her", Robust.Shared.Enums.Gender.Female },
            { "They/Them", Robust.Shared.Enums.Gender.Epicene },
            { "It/Its", Robust.Shared.Enums.Gender.Neuter }
        };

}
