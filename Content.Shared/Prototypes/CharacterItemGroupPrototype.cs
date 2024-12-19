using System.Diagnostics.CodeAnalysis;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Clothing.Loadouts.Systems;
using Content.Shared.Customization.Systems;
using Content.Shared.Preferences;
using Content.Shared.Traits;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Prototypes;

[Prototype]
public sealed partial class CharacterItemGroupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    /// How many items from this group can be selected at once
    [DataField]
    public int MaxItems = 1;

    /// The minimum amount of items required from this group.
    [DataField]
    public int MinItems = 0;

    /// The character requirements applied to all items in this group.
    [DataField]
    public List<CharacterRequirement> Requirements = new();

    /// An arbitrary list of traits, loadouts, etc
    [DataField]
    public List<CharacterItemGroupItem> Items = new();
}

[DataDefinition]
public sealed partial class CharacterItemGroupItem
{
    [DataField(required: true)]
    public string Type;

    [DataField("id", required: true)]
    public string ID;

    /// The priority for this item to be selected as the default item.
    /// The higher priority items will always be selected first over
    /// lower-priority items.
    [DataField]
    public int Priority = 0;

    /// Tries to get Value from whatever Type maps to on a character profile
    //TODO: Make a test for this
    public bool TryGetValue(HumanoidCharacterProfile profile, IPrototypeManager protoMan, [NotNullWhen(true)] out object? value)
    {
        value = null;

        // This sucks
        switch (Type)
        {
            case "trait":
                return profile.TraitPreferences.TryFirstOrDefault(
                    p => protoMan.Index<TraitPrototype>((string) p).ID == ID, out value);
            case "loadout":
                return profile.LoadoutPreferences.TryFirstOrDefault(
                    p => protoMan.Index<LoadoutPrototype>(((Loadout) p).LoadoutName).ID == ID, out value);
            default:
                DebugTools.Assert($"Invalid CharacterItemGroupItem Type: {Type}");
                return false;
        }
    }
}
