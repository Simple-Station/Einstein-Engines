using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared.Bed.Sleep;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Drugs;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Crescent.Psionics;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server.Crescent.Chat
{
    /// <summary>
    /// Telepathy
    /// </summary>

    public sealed class TChatSystem : EntitySystem
    {
        [Dependency] private readonly IAdminManager _adminManager = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        private IEnumerable<INetChannel> GetPsionicChatClients()
        {
            return Filter.Empty()
                .AddWhereAttachedEntity(IsEligibleForTelepathy)
                .Recipients
                .Select(p => p.Channel);
        }

        private IEnumerable<INetChannel> GetAdminClients()
        {
            return _adminManager.ActiveAdmins
                .Select(p => p.Channel);
        }

        private bool IsEligibleForTelepathy(EntityUid entity)
        {
            return HasComp<TelepathicComponent>(entity)
                && (!TryComp<MobStateComponent>(entity, out var mobstate) || mobstate.CurrentState != MobState.Dead);
        }

        public void SendTelepathicChat(EntityUid source, string message, bool hideChat)
        {
            if (!IsEligibleForTelepathy(source))
                return;

            var clients = GetPsionicChatClients();
            var admins = GetAdminClients();
            string messageWrap;
            string adminMessageWrap;

            messageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message",
                ("telepathicChannelName", Loc.GetString("chat-manager-telepathic-channel-name")), ("message", message));

            adminMessageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message-admin",
                ("source", source), ("message", message));

            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Telepathic chat from {ToPrettyString(source):Player}: {message}");

            _chatManager.ChatMessageToMany(ChatChannel.Telepathic, message, messageWrap, source, hideChat, true, clients.ToList(), Color.PaleVioletRed);

            _chatManager.ChatMessageToMany(ChatChannel.Telepathic, message, adminMessageWrap, source, hideChat, true, admins, Color.PaleVioletRed);

            foreach (var repeater in EntityQuery<TelepathicRepeaterComponent>())
            {
                _chatSystem.TrySendInGameICMessage(repeater.Owner, message, InGameICChatType.Speak, false);
            }
        }
    }
}
