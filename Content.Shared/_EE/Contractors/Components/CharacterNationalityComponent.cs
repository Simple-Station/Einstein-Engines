using Content.Shared._EE.Contractors.Prototypes;

namespace Content.Shared._EE.Contractors.Components;

/// <summary>
/// Contains a character's nationality prototype
/// </summary>
[RegisterComponent]
public sealed partial  class CharacterNationalityComponent : Component
{
    [DataField(required: true)]
    public NationalityPrototype Nationality;
}
