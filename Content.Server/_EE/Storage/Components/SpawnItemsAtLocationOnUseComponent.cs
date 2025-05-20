using Content.Shared.Storage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._EE.Storage.Components;

/// <summary>
/// Spawns items at the entity's location when used, with rotation alignment and a text menu.
/// I recommend not using it on normal items that fit in the hand; use SpawnItemsOnUse if that's the case.
/// </summary>
[RegisterComponent]
public sealed partial class SpawnItemsAtLocationOnUseComponent : Component
{
    /// <summary>
    /// The list of entities to spawn, with amounts and orGroups.
    /// </summary>
    [DataField("items", required: true)]
    public List<EntitySpawnEntry> Items = new();

    /// <summary>
    /// A sound to play when the items are spawned.
    /// </summary>
    [DataField("sound")]
    public SoundSpecifier? Sound;

    /// <summary>
    /// How many times the entity can be used before deleting itself.
    /// </summary>
    [DataField("uses")]
    public int Uses = 1;

    /// <summary>
    /// Localization ID for the verb text shown in the UI.
    /// </summary>
    [DataField]
    public LocId SpawnItemsVerbText = "spawn-items-verb";
}
