using Content.Shared._EE.Contractors.Prototypes;

namespace Content.Shared._EE.Contractors.Components;


/// <summary>
/// Contains a character's employer prototype
/// </summary>
[RegisterComponent]
public sealed partial class CharacterEmployerComponent : Component
{
    [DataField(required: true)]
    public EmployerPrototype Employer;
}
