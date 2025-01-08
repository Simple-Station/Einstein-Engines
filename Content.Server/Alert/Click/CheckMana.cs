using Content.Server.Chat.Managers;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Alert;
using Content.Shared.Chat;
using JetBrains.Annotations;
using Robust.Server.Player;
using Robust.Shared.Player;

namespace Content.Server.Alert.Click;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CheckMana : IAlertClick
{
    public void AlertClicked(EntityUid player)
    {
        var chatManager = IoCManager.Resolve<IChatManager>();
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var playerManager = IoCManager.Resolve<IPlayerManager>();

        if (!entityManager.TryGetComponent(player, out PsionicComponent? magic) ||
            !playerManager.TryGetSessionByEntity(player, out var session))
            return;

        var baseMsg = Loc.GetString("mana-alert", ("mana", magic.Mana), ("manaMax", magic.MaxMana));
        SendMessage(chatManager, baseMsg, session);
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
