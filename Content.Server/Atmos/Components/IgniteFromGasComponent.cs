using Content.Shared.Atmos;

namespace Content.Server.Atmos.Components;

/// <summary>
///   Component that can be used to add (or remove) fire stacks when exposed to a type of gas, unless wearing ignition immunity.
/// </summary>
[RegisterComponent]
public sealed partial class IgniteFromGasComponent : Component
{
    /// <summary>
    ///   These are the inventory slot groups that are checked for ignition immunity.
    ///   If no slot in a group has IgniteFromGasImmunityComponent, no immunity is applied.
    /// </summary>
    [DataField]
    public List<List<String>> ImmunitySlotGroups = new() {
        new () { "head" },
        new () { "jumpsuit", "outerClothing" }
    };

    /// <summary>
    ///   What type of gas triggers ignition.
    /// </summary>
    [DataField(required: true)]
    public Gas Gas;

    /// <summary>
    ///   How many firestacks to apply every second without immunity.
    /// </summary>
    [DataField(required: true)]
    public float FireStacks;

    /// <summary>
    ///   How many moles of the gas is needed to trigger ignition.
    /// </summary>
    [DataField(required: true)]
    public float MolesToIgnite;

    /// <summary>
    /// Whether the entity is immune to ignition.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool HasImmunity = false;
}
