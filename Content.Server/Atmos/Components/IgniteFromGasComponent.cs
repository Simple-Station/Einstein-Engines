using Content.Shared.Atmos;
using Content.Shared.Body.Part;

namespace Content.Server.Atmos.Components;

/// <summary>
///   Component that can be used to add (or remove) fire stacks when exposed to a type of gas, unless wearing ignition immunity.
/// </summary>
[RegisterComponent]
public sealed partial class IgniteFromGasComponent : Component
{
    /// <summary>
    ///   What type of gas triggers ignition.
    /// </summary>
    [DataField(required: true)]
    public Gas Gas;

    /// <summary>
    ///   The total calculated fire stacks to apply every second without immunity.
    ///   This is calculated from BaseFireStacks + the exposed body parts' fire stacks
    ///   from IgnitableBodyParts.
    /// </summary>
    [DataField]
    public float FireStacks = 0f;

    /// <summary>
    ///   The base amount of fire stacks to apply every second without immunity.
    /// </summary>
    [DataField]
    public float BaseFireStacks = 0.13f;

    /// <summary>
    ///   The body parts that are vulnerable to ignition when exposed, and their fire stack values.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<(BodyPartType, BodyPartSymmetry), float> IgnitableBodyParts = default!;

    /// <summary>
    ///   How many moles of the gas is needed to trigger ignition.
    /// </summary>
    [DataField]
    public float MolesToIgnite = 0.5f;

    /// <summary>
    ///   Whether the entity is currently immune to ignition.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool HasImmunity = false;
}
