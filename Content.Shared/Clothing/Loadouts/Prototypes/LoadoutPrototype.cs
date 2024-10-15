using Content.Shared.Clothing.Loadouts.Systems;
using Content.Shared.Customization.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Clothing.Loadouts.Prototypes;


[Prototype]
public sealed partial class LoadoutPrototype : IPrototype
{
    /// <summary>
    ///     Formatted like "Loadout[Department/ShortHeadName][CommonClothingSlot][SimplifiedClothingId]", example: "LoadoutScienceOuterLabcoatSeniorResearcher"
    /// </summary>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField, ValidatePrototypeId<LoadoutCategoryPrototype>]
    public ProtoId<LoadoutCategoryPrototype> Category = "Uncategorized";

    [DataField(required: true)]
    public List<ProtoId<EntityPrototype>> Items = new();

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
