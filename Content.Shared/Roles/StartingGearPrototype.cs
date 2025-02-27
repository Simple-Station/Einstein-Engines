using Content.Shared.Customization.Systems;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared.Roles;

[Prototype("startingGear")]
public sealed partial class StartingGearPrototype : IPrototype, IInheritingPrototype
{
    [DataField]
    [AlwaysPushInheritance]
    public Dictionary<string, EntProtoId> Equipment = new();

    /// <summary>
    ///     If empty, there is no skirt override - instead the uniform provided in equipment is added.
    /// </summary>
    [DataField]
    public EntProtoId? InnerClothingSkirt;

    [DataField]
    public EntProtoId? Satchel;

    [DataField]
    public EntProtoId? Duffelbag;

    [DataField]
    [AlwaysPushInheritance]
    public List<EntProtoId> Inhand = new(0);

    /// <summary>
    ///     Inserts entities into the specified slot's storage (if it does have storage).
    /// </summary>
    [DataField]
    [AlwaysPushInheritance]
    public Dictionary<string, List<EntProtoId>> Storage = new();

    /// <summary>
    ///     The list of starting gears that overwrite the entries on this starting gear
    ///     if their requirements are satisfied.
    /// </summary>
    [DataField("subGear")]
    [AlwaysPushInheritance]
    public List<ProtoId<StartingGearPrototype>> SubGears = new();

    /// <summary>
    ///     The requirements of this starting gear.
    ///     Only used if this starting gear is a sub-gear of another starting gear.
    /// </summary>
    [DataField]
    [AlwaysPushInheritance]
    public List<CharacterRequirement> Requirements = new();

    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    /// <inheritdoc/>
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<StartingGearPrototype>))]
    public string[]? Parents { get; private set; }

    /// <inheritdoc/>
    [AbstractDataField]
    [NeverPushInheritance]
    public bool Abstract { get; }

    public string GetGear(string slot, HumanoidCharacterProfile? profile)
    {
        if (profile != null)
        {
            switch (slot)
            {
                case "jumpsuit" when profile.Clothing == ClothingPreference.Jumpskirt && !string.IsNullOrEmpty(InnerClothingSkirt):
                case "jumpsuit" when profile.Species == "Harpy" && !string.IsNullOrEmpty(InnerClothingSkirt):
                case "jumpsuit" when profile.Species == "Lamia" && !string.IsNullOrEmpty(InnerClothingSkirt):
                    return InnerClothingSkirt;
                case "back" when profile.Backpack == BackpackPreference.Satchel && !string.IsNullOrEmpty(Satchel):
                    return Satchel;
                case "back" when profile.Backpack == BackpackPreference.Duffelbag && !string.IsNullOrEmpty(Duffelbag):
                    return Duffelbag;
            }
        }

        return Equipment.TryGetValue(slot, out var equipment) ? equipment : string.Empty;
    }
}
