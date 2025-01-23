using Content.Server.Chat.Systems;
using Content.Shared.WhiteDream.BloodCult.Items.BaseAura;
using Content.Shared.WhiteDream.BloodCult.Spells;

namespace Content.Server.WhiteDream.BloodCult.Items.BaseAura;

public abstract class BaseAuraSystem<T> : EntitySystem where T : BaseAuraComponent
{
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<T, SpeakOnAuraUseEvent>(OnAuraUse);
    }

    private void OnAuraUse(EntityUid uid, T component, SpeakOnAuraUseEvent args)
    {
        // TODO: The charge of the aura spell should be spent here
        if (component.Speech != null)
            _chat.TrySendInGameICMessage(args.User, component.Speech, component.ChatType, false);
    }
}
