using Content.Shared.Clothing.Loadouts.Systems;
using Content.Shared.Customization.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Clothing.Loadouts.Prototypes;


[Prototype("loadout")]
public sealed class LoadoutPrototype : IPrototype
{
    /// <summary>
    ///     Formatted like "Loadout[Department/ShortHeadName][CommonClothingSlot][SimplifiedClothingId]", example: "LoadoutScienceOuterLabcoatSeniorResearcher"
    /// </summary>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    ///     Which tab category to put this under
    /// </summary>
    [DataField, ValidatePrototypeId<LoadoutCategoryPrototype>]
    public string Category = "Uncategorized";

    /// <summary>
    ///     The item to give
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>), required: true)]
    public List<string> Items = new();


    /// <summary>
    ///     The point cost of this loadout
    /// </summary>
    [DataField]
    public int Cost = 1;

    /// <summary>
    ///     Should this item override other items in the same slot?
    /// </summary>
    [DataField]
    public bool Exclusive;


    [DataField]
    public List<CharacterRequirement> Requirements = new();
}
