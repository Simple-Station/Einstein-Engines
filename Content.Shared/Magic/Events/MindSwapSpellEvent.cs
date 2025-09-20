using Content.Shared.Actions;
using Content.Shared.Chat;

namespace Content.Shared.Magic.Events;

public sealed partial class MindSwapSpellEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public TimeSpan PerformerStunDuration = TimeSpan.FromSeconds(10);

    [DataField]
    public TimeSpan TargetStunDuration = TimeSpan.FromSeconds(10);

    [DataField]
    public string? Speech { get; private set; }

    public InGameICChatType ChatType => InGameICChatType.Speak;
}
