using Content.Shared.NPC.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.NPC.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(NpcFactionSelectorSystem))]
public sealed partial class NpcFactionSelectorComponent : Component
{
    [DataField]
    public List<string> SelectableFactions = new();
}
