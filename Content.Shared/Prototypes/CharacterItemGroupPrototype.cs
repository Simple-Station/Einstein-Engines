using System.Diagnostics.CodeAnalysis;
using Content.Shared.Clothing.Loadouts.Prototypes;
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

    /// Tries to get Value from whatever Type maps to on a character profile
    //TODO: Make a test for this
    public bool TryGetValue(HumanoidCharacterProfile profile, IPrototypeManager protoMan, [NotNullWhen(true)] out object? value)
    {
        value = null;

        // This sucks
        // TODO what the fuck is this method supposed to do? The output value is always equal to the ID property, AND IS IGNORED IN ALL EXECUTION PATHS
        // god save me
        switch (Type)
        {
            case "trait":
                // TODO why the hell is this done? The ID of the prototype is always equal to its own ID, why even index it?
                return profile.TraitPreferences.TryFirstOrDefault(
                    p => protoMan.Index<TraitPrototype>((string) p).ID == ID,
                    out value);
            case "loadout":
                if (!profile.LoadoutPreferences.TryGetLoadout(out var loadout) || !loadout.Items.Contains(ID))
                    return false;

                value = ID;
                return true;
            default:
                DebugTools.Assert($"Invalid CharacterItemGroupItem Type: {Type}");
                return false;
        }
    }
}
