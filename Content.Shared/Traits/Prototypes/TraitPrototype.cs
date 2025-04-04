using Content.Shared.Customization.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared.Traits;


/// <summary>
///     Describes a trait.
/// </summary>
[Prototype("trait")]
public sealed partial class TraitPrototype : IPrototype, IComparable
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     Which customization tab to place this entry in
    /// </summary>
    [DataField(required: true)]
    public ProtoId<TraitCategoryPrototype> Category = "Uncategorized";

    /// <summary>
    ///     How many points this will give the character
    /// </summary>
    [DataField]
    public int Points = 0;


    [DataField]
    public List<CharacterRequirement> Requirements = new();

    [DataField(serverOnly: true)]
    public TraitFunction[] Functions { get; private set; } = Array.Empty<TraitFunction>();

    /// <summary>
    ///     Should this trait be loaded earlier/later than other traits?
    /// </summary>
    [DataField]
    public int Priority = 0;
    public int CompareTo(object? obj) // Compare function to allow for some traits to specify they need to load earlier than others
    {
        if (obj is not TraitPrototype other)
            return -1;

        return Priority.CompareTo(other.Priority); // No need for total ordering, only care about things that want to be loaded earlier or later.
    }
}

/// This serves as a hook for trait functions to modify a player character upon spawning in.
[ImplicitDataDefinitionForInheritors]
public abstract partial class TraitFunction
{
    public abstract void OnPlayerSpawn(
        EntityUid mob,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager);
}
