using Content.Shared.Containers.ItemSlots;
using Content.Shared.RadialSelector;

namespace Content.Server.WhiteDream.BloodCult.Constructs.Shell;

[RegisterComponent]
public sealed partial class ConstructShellComponent : Component
{
    [DataField(required: true)]
    public ItemSlot ShardSlot = new();

    public readonly string ShardSlotId = "Shard";


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
}
