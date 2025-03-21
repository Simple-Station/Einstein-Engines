using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Customization.Systems;

namespace Content.Shared._EE.Contractors.Prototypes;

/// <summary>
/// Prototype representing a character's nationality in YAML.
/// </summary>
[Prototype("Nationality")]
public sealed partial class NationalityPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; } = string.Empty;

    [DataField]
    public string DescriptionKey { get; } = string.Empty;

    [DataField, ViewVariables]
    public List<NationalityPrototype> Allied { get; } = new();

    [DataField, ViewVariables]
    public List<NationalityPrototype> Hostile { get; } = new();

    [DataField]
    public List<CharacterRequirement> Requirements = new();

    [DataField(serverOnly: true)]
    public NationalityFunction[] Functions { get; private set; } = Array.Empty<NationalityFunction>();
}

/// This serves as a hook for trait functions to modify a player character upon spawning in.
[ImplicitDataDefinitionForInheritors]
public abstract partial class NationalityFunction
{
    public abstract void OnPlayerSpawn(
        EntityUid mob,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager);
}
