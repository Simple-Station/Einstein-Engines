using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Customization.Systems;

namespace Content.Shared._EE.Contractors.Prototypes;

/// <summary>
/// Prototype representing a character's Lifepath in YAML.
/// </summary>
[Prototype("Lifepath")]
public sealed partial class LifepathPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; } = string.Empty;

    [DataField]
    public List<CharacterRequirement> Requirements = new();

    [DataField(serverOnly: true)]
    public LifepathFunction[] Functions { get; private set; } = Array.Empty<LifepathFunction>();
}

/// This serves as a hook for trait functions to modify a player character upon spawning in.
[ImplicitDataDefinitionForInheritors]
public abstract partial class LifepathFunction
{
    public abstract void OnPlayerSpawn(
        EntityUid mob,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager);
}
