using Content.Shared.Actions;
using Content.Shared.Chat;
using Robust.Shared.Audio;
using Content.Shared.Magic;

namespace Content.Shared._Crescent.Magic;

public sealed partial class LesserKnockdownSpellEvent : WorldTargetActionEvent, ISpeakSpell
{
    /// <summary>
    /// Sound effect for the spell.
    /// </summary>
    [DataField("sound")]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Magic/fireball.ogg");

    /// <summary>
    /// Volume control for the spell.
    /// </summary>
    [DataField("volume")]
    public float Volume = 5f;

    [DataField("range")]
    public float Range = 3f;

    [DataField("knockdownForce")]
    public float KnockdownForce = 50f;

    [DataField("staminaDamage")]
    public float StaminaDamage = 90f;

    [DataField("speech")]
    public string? Speech { get; private set; }

    [DataField]
    public InGameICChatType ChatType { get; private set; }
}
