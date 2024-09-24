using Robust.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Popups;

namespace Content.Shared.Actions.Events;
public sealed partial class PsionicHealOtherPowerActionEvent : EntityTargetActionEvent
{
    [DataField]
    public DamageSpecifier? HealingAmount = default!;

    [DataField]
    public string PowerName;

    /// Controls whether or not a power fires immediately and with no DoAfter
    [DataField]
    public bool Immediate;

    [DataField]
    public string? PopupText;

    [DataField]
    public float? RotReduction;

    [DataField]
    public bool DoRevive;

    [DataField]
    public bool BreakOnUserMove = true;

    [DataField]
    public bool BreakOnTargetMove = false;

    [DataField]
    public float UseDelay = 8f;

    [DataField]
    public int MinGlimmer = 8;

    [DataField]
    public int MaxGlimmer = 12;

    [DataField]
    public int GlimmerSoundThreshold;

    [DataField]
    public int GlimmerPopupThreshold;

    [DataField]
    public int GlimmerDoAfterVisibilityThreshold;

    [DataField]
    public PopupType PopupType = PopupType.Medium;

    [DataField]
    public AudioParams AudioParams = default!;

    [DataField]
    public bool PlaySound;

    [DataField]
    public SoundSpecifier SoundUse = new SoundPathSpecifier("/Audio/Psionics/heartbeat_fast.ogg");
}
