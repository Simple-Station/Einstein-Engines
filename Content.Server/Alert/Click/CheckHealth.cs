using Content.Server.Chat.Managers;
using Content.Shared.Alert;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.HealthExaminable;
using JetBrains.Annotations;
using Robust.Server.Player;
using Robust.Shared.Player;

namespace Content.Server.Alert.Click;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CheckHealth : IAlertClick
{
    public void AlertClicked(EntityUid player)
    {
        var chatManager = IoCManager.Resolve<IChatManager>();
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var playerManager = IoCManager.Resolve<IPlayerManager>();

        var healthExaminableSystem = entityManager.System<HealthExaminableSystem>();

        if (!entityManager.TryGetComponent(player, out HealthExaminableComponent? healthExaminable) ||
            !entityManager.TryGetComponent(player, out DamageableComponent? damageable) ||
            !playerManager.TryGetSessionByEntity(player, out var session))
            return;

        var baseMsg = Loc.GetString("health-alert-start");
        SendMessage(chatManager, baseMsg, session);
        var markup = healthExaminableSystem.GetMarkup(player, (player, healthExaminable), damageable).ToMarkup();
        SendMessage(chatManager, markup, session);
    }

    private static void SendMessage(IChatManager chatManager, string msg, ICommonSession session)
    {
        chatManager.ChatMessageToOne(ChatChannel.Emotes,
            msg,
            msg,
            EntityUid.Invalid,
            false,
            session.Channel);
    }
}
