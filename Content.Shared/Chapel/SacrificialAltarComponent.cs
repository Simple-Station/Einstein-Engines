using Content.Shared.Random;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chapel;

/// <summary>
///     Altar that lets you sacrifice psionics to lower glimmer by a large amount.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSacrificialAltarSystem))]
public sealed partial class SacrificialAltarComponent : Component
{
    /// <summary>
    ///     DoAfter for an active sacrifice.
    /// </summary>
    [DataField]
    public DoAfterId? DoAfter;

    /// <summary>
    ///     How long it takes to sacrifice someone once they die.
    ///     This is the window to interrupt a sacrifice if you want glimmer to stay high, or need the psionic to be revived.
    /// </summary>
    [DataField]
    public TimeSpan SacrificeTime = TimeSpan.FromSeconds(8.35);

    [DataField]
    public SoundSpecifier SacrificeSound = new SoundPathSpecifier("/Audio/Psionics/heartbeat_fast.ogg");

    [DataField]
    public EntityUid? SacrificeStream;

    /// <summary>
    ///     Base amount to reduce glimmer by, multiplied by the victim's Amplification stat.
    /// </summary>
    [DataField]
    public float GlimmerReduction = -25;

    [DataField]
    public List<ProtoId<WeightedRandomEntityPrototype>>? RewardPool;

    /// <summary>
    ///     The base chance to generate an item of power, multiplied by the victim's Dampening stat.
    /// </summary>
    [DataField]
    public float BaseItemChance = 0.1f;
}
