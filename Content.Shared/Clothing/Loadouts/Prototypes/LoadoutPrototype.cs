using Content.Shared.Customization.Systems;
using Content.Shared.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Clothing.Loadouts.Prototypes;


[Prototype]
public sealed class LoadoutPrototype : LocalizedPrototype
{
    /// <summary>The localization string for the description of this loadout</summary>
    public string DescriptionLoc => ToLocalizationString("desc");
    /// <summary>The localized string for the description of this loadout</summary>
    public string Description => Loc.GetString(DescriptionLoc);

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
