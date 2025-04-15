using Content.Shared.Actions;
using Robust.Shared.Audio;
using Content.Shared.Magic;
using Content.Shared.FixedPoint;
using Content.Shared.Damage;

namespace Content.Shared._Crescent.Magic;
public sealed partial class GreaterSuppressSpellEvent : InstantActionEvent, ISpeakSpell
{
    /// <summary>
    /// Sound effect for the spell.
    /// </summary>
    [DataField("Sound")]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/singularity_collapse.ogg");

    /// <summary>
    /// Volume control for the spell.
    /// </summary>
    [DataField("Volume")]
    public float Volume = 10f;

    [DataField("range")]
    public float Range = 25f;

    [DataField("bleedStacks")]
    public float BleedStacks = 10f;

    [DataField("selfDamage")]
    public float SelfDamage = 5f;

    [DataField("staminaDamage")]
    public float StaminaDamage = 90f;

    [DataField("speech")]
    public string? Speech { get; private set; }
}
