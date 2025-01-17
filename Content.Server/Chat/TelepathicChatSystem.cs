using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Psionics.Passives;
using Content.Shared.Bed.Sleep;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Drugs;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using System.Linq;
using System.Text;

namespace Content.Server.Chat;

/// <summary>
/// Extensions for Telepathic chat stuff
/// </summary>
public sealed partial class TelepathicChatSystem : EntitySystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializePsychognomy();
    }

    private (IEnumerable<INetChannel> normal, IEnumerable<INetChannel> psychog) GetPsionicChatClients()
    {
        var psions = Filter.Empty()
            .AddWhereAttachedEntity(IsEligibleForTelepathy)
            .Recipients;

        var normalSessions = psions.Where(p => !HasComp<PsychognomistComponent>(p.AttachedEntity)).Select(p => p.Channel);
        var psychogSessions = psions.Where(p => HasComp<PsychognomistComponent>(p.AttachedEntity)).Select(p => p.Channel);

        return (normalSessions, psychogSessions);
    }

    private IEnumerable<INetChannel> GetAdminClients()
    {
        return _adminManager.ActiveAdmins
            .Select(p => p.Channel);
    }

    private List<INetChannel> GetDreamers(IEnumerable<INetChannel> removeList)
    {
        var filteredList = new List<INetChannel>();
        var filtered = Filter.Empty()
            .AddWhereAttachedEntity(entity =>
                HasComp<PsionicComponent>(entity) && !HasComp<TelepathyComponent>(entity)
                || HasComp<SleepingComponent>(entity)
                || HasComp<SeeingRainbowsComponent>(entity) && !HasComp<PsionicsDisabledComponent>(entity) && !HasComp<PsionicInsulationComponent>(entity))
            .Recipients
            .Select(p => p.Channel);

        if (filtered.ToList() != null)
            filteredList = filtered.ToList();

        foreach (var entity in removeList)
            filteredList.Remove(entity);

        return filteredList;
    }

    private bool IsEligibleForTelepathy(EntityUid entity)
    {
        return HasComp<TelepathyComponent>(entity)
            && !HasComp<PsionicsDisabledComponent>(entity)
            && !HasComp<PsionicInsulationComponent>(entity)
            && (!TryComp<MobStateComponent>(entity, out var mobstate) || mobstate.CurrentState == MobState.Alive);
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

        _chatManager.ChatMessageToMany(ChatChannel.Telepathic, message, messageWrap, source, hideChat, true, clients.normal.ToList(), Color.PaleVioletRed);

        _chatManager.ChatMessageToMany(ChatChannel.Telepathic, message, adminMessageWrap, source, hideChat, true, admins, Color.PaleVioletRed);

        if (clients.psychog.Count() > 0)
        {
            var descriptor = SourceToDescriptor(source);
            string psychogMessageWrap;

            psychogMessageWrap = Loc.GetString("chat-manager-send-telepathic-chat-wrap-message-psychognomy",
                ("source", descriptor.ToUpper()), ("message", message));

            _chatManager.ChatMessageToMany(ChatChannel.Telepathic, message, psychogMessageWrap, source, hideChat, true, clients.psychog.ToList(), Color.PaleVioletRed);
        }

        if (_random.Prob(0.1f))
            _glimmerSystem.DeltaGlimmerInput(1);

        if (_random.Prob(Math.Min(0.33f + (float) _glimmerSystem.GlimmerOutput / 1500, 1)))
        {
            float obfuscation = 0.25f + (float) _glimmerSystem.GlimmerOutput / 2000;
            var obfuscated = ObfuscateMessageReadability(message, obfuscation);
            _chatManager.ChatMessageToMany(ChatChannel.Telepathic, obfuscated, messageWrap, source, hideChat, false, GetDreamers(clients.normal.Concat(clients.psychog)), Color.PaleVioletRed);
        }

        foreach (var repeater in EntityQuery<TelepathicRepeaterComponent>())
            _chatSystem.TrySendInGameICMessage(repeater.Owner, message, InGameICChatType.Speak, false);
    }

    private string ObfuscateMessageReadability(string message, float chance)
    {
        var modifiedMessage = new StringBuilder(message);

        for (var i = 0; i < message.Length; i++)
        {
            if (char.IsWhiteSpace(modifiedMessage[i]))
            {
                continue;
            }

            if (_random.Prob(1 - chance))
            {
                modifiedMessage[i] = '~';
            }
        }

        return modifiedMessage.ToString();
    }
}
