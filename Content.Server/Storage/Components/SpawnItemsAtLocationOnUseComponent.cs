using Content.Shared.Storage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.Storage.Components;

[RegisterComponent]
public sealed partial class SpawnItemsAtLocationOnUseComponent : Component
{
    [DataField("items", required: true)]
    public List<EntitySpawnEntry> Items = new();

    [DataField("sound")]
    public SoundSpecifier? Sound;

    [DataField("uses")]
    public int Uses = 1;

    [DataField]
    public LocId VerbText = "spawn-items-verb";
}
