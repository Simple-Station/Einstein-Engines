using Content.Shared.Customization.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared._EE.Contractors.Prototypes;

/// <summary>
/// Prototype representing a character's employer in YAML.
/// </summary>
[Prototype("Employer")]
public sealed partial class EmployerPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; } = string.Empty;

    [DataField]
    public List<CharacterRequirement> Requirements = new();

    [DataField(serverOnly: true)]
    public EmployerFunction[] Functions { get; private set; } = Array.Empty<EmployerFunction>();
}

/// This serves as a hook for trait functions to modify a player character upon spawning in.
[ImplicitDataDefinitionForInheritors]
public abstract partial class EmployerFunction
{
    public abstract void OnPlayerSpawn(
        EntityUid mob,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager);
}
