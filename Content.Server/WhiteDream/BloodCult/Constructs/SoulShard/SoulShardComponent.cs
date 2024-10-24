using Content.Shared.RadialSelector;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Constructs.SoulShard;

[RegisterComponent]
public sealed partial class SoulShardComponent : Component
{
    [DataField]
    public bool IsBlessed;

    [DataField]
    public Color BlessedLightColor = Color.LightCyan;

    [DataField]
    public EntProtoId ShadeProto = "ShadeCult";

    [DataField]
    public EntProtoId PurifiedShadeProto = "ShadeHoly";

    [DataField]
    public List<RadialSelectorEntry> Constructs = new()
    {
        new RadialSelectorEntry
        {
            Prototype = "ConstructJuggernaut"
        },
        new RadialSelectorEntry
        {
            Prototype = "ConstructArtificer"
        },
        new RadialSelectorEntry
        {
            Prototype = "ConstructWraith"
        }
    };

    [DataField]
    public List<RadialSelectorEntry> PurifiedConstructs = new()
    {
        new RadialSelectorEntry
        {
            Prototype = "ConstructJuggernautHoly"
        },
        new RadialSelectorEntry
        {
            Prototype = "ConstructArtificerHoly"
        },
        new RadialSelectorEntry
        {
            Prototype = "ConstructWraithHoly"
        }
    };

    public EntityUid? ShadeUid;
}
