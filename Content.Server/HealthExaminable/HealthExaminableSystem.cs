using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.HealthExaminable;
using Robust.Server.Player;

namespace Content.Server.HealthExaminable;

public sealed class HealthExaminableSystem : SharedHealthExaminableSystem
{
    [Dependency] private readonly IChatManager _сhatManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HealthExaminableComponent, CheckHealthAlertEvent>(OnCheckHealthAlert);
    }

    private void OnCheckHealthAlert(EntityUid uid, HealthExaminableComponent component, CheckHealthAlertEvent args)
    {
        if (!TryComp(uid, out DamageableComponent? damageable)
            || !_playerManager.TryGetSessionByEntity(uid, out var session))
            return;

        var markup = Loc.GetString("health-alert-start")
            + "\n"
            + GetMarkup(uid, (uid, component), damageable).ToMarkup();

        _сhatManager.ChatMessageToOne(
            ChatChannel.Emotes,
            markup,
            markup,
            EntityUid.Invalid,
            false,
            session.Channel);
    }
}
