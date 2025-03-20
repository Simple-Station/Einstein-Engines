using Robust.Shared.Prototypes;

namespace Content.Shared._EE.Contractors.Prototypes;

/// <summary>
/// Prototype representing a character's nationality in YAML.
/// </summary>
[Prototype("Nationality")]
public sealed partial class NationalityPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; } = string.Empty;

    [DataField, ViewVariables]
    public List<NationalityPrototype> Allied { get; } = new();

    [DataField, ViewVariables]
    public List<NationalityPrototype> Hostile { get; } = new();

    [DataField, ViewVariables]
    public List<EmployerPrototype> Employers { get; } = new();
}
