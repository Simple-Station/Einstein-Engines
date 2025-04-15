using Content.Shared.Actions;
using Content.Shared.Chat;
using Robust.Shared.Audio;
using Content.Shared.Magic;
using Content.Shared.FixedPoint;

namespace Content.Shared._Crescent.Magic;
public sealed partial class LesserSuppressSpellEvent : WorldTargetActionEvent, ISpeakSpell
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
    public float Volume = 5f;

    [DataField("range")]
    public float Range = 4f;

    [DataField("bleedStacks")]
    public float BleedStacks = 10f;

    [DataField("speech")]
    public string? Speech { get; private set; }

    [DataField]
    public InGameICChatType ChatType { get; private set; }
}
