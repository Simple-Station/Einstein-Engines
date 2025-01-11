using Content.Shared.Chat;

namespace Content.Shared.WhiteDream.BloodCult.Items.BaseAura;

public abstract partial class BaseAuraComponent : Component
{
    [DataField]
    public string? Speech;

    [DataField]
    public InGameICChatType ChatType = InGameICChatType.Whisper;
}
