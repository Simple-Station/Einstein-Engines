using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Nutrition.Prototypes;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared.Nutrition.Components;

/// <summary>
/// Indicates that this entity can be inserted into FoodSequence, which will transfer all reagents to the target.
/// </summary>
[RegisterComponent, Access(typeof(SharedFoodSequenceSystem))]
public sealed partial class FoodSequenceElementComponent : Component
{
    /// <summary>
    /// The same object can be used in different sequences, and it will have a different data in then.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<ProtoId<TagPrototype>, ProtoId<FoodSequenceElementPrototype>> Entries = new();

    /// <summary>
    /// Which solution we will add to the main dish
    /// </summary>
    [DataField]
    public string Solution = "food";
}

[DataRecord, Serializable, NetSerializable]
public partial record struct FoodSequenceElementEntry()
{
    /// <summary>
    /// A localized name piece to build into the item name generator.
    /// </summary>
    public LocId? Name { get; set; } = null;

    /// <summary>
    /// state used to generate the appearance of the added layer
    /// </summary>
    public SpriteSpecifier? Sprite { get; set; } = null;

    /// <summary>
    /// If the layer is the final one, it can be added over the limit, but no other layers can be added after it.
    /// </summary>
    public bool Final { get; set; } = false;
}
