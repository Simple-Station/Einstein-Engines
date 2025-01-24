using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wires.Components;

/// This is used for items slots that require entity to have wire panel for interactions
[RegisterComponent]
[NetworkedComponent]
public sealed partial class ItemSlotsRequirePanelComponent : Component
{
    /// For each slot: true - slot require opened panel for interaction, false - slot require closed panel for interaction
    [DataField]
    public Dictionary<string, bool> Slots = new();
}
