using Robust.Shared.Prototypes;

namespace Content.Shared._EE.Contractors.Prototypes;

/// <summary>
/// Prototype representing a character's employer in YAML.
/// </summary>
[Prototype("Employer")]
public sealed partial class EmployerPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; } = string.Empty;
}
