using Content.Shared.Actions;
using Content.Shared.Chat;
using Robust.Shared.Audio;
using Content.Shared.Magic;

namespace Content.Shared._Crescent.Magic;

public sealed partial class LesserHackSpellEvent : EntityTargetActionEvent, ISpeakSpell
{
    /// <summary>
    /// Sound effect for the spell.
    /// </summary>
    [DataField("sound")]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Magic/knock.ogg");

    /// <summary>
    /// Volume control for the spell.
    /// </summary>
    [DataField("volume")]
    public float Volume = 5f;

    [DataField("speech")]
    public string? Speech { get; private set; }

    [DataField]
    public InGameICChatType ChatType { get; private set; }
}
