using Content.Server.Chat.Managers;
using Content.Shared.Alert;
using Content.Shared.Chat;
using Content.Shared.Mind;
using Content.Shared.Mood;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.Mood;

[UsedImplicitly]
[DataDefinition]
public sealed partial class ShowMoodEffects : IAlertClick
{
    public void AlertClicked(EntityUid uid)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var chatManager = IoCManager.Resolve<IChatManager>();

        if (!entityManager.TryGetComponent<MoodComponent>(uid, out var comp)
            || comp.CurrentMoodThreshold == MoodThreshold.Dead
            || !entityManager.TryGetComponent<MindComponent>(uid, out var mindComp)
            || mindComp.Session == null)
            return;

        var msgStart = Loc.GetString("mood-show-effects-start");
        chatManager.ChatMessageToOne(ChatChannel.Emotes, msgStart, msgStart, EntityUid.Invalid, false,
            mindComp.Session.Channel);

        foreach (var (_, protoId) in comp.CategorisedEffects)
        {
            if (!prototypeManager.TryIndex<MoodEffectPrototype>(protoId, out var proto)
                || proto.Hidden)
                continue;

            SendDescToChat(proto, mindComp);
        }

        foreach (var (protoId, _) in comp.UncategorisedEffects)
        {
            if (!prototypeManager.TryIndex<MoodEffectPrototype>(protoId, out var proto)
                || proto.Hidden)
                continue;

            SendDescToChat(proto, mindComp);
        }
    }

    private void SendDescToChat(MoodEffectPrototype proto, MindComponent comp)
    {
        if (comp.Session == null)
            return;

        var chatManager = IoCManager.Resolve<IChatManager>();

        var color = proto.PositiveEffect ? "#008000" : "#BA0000";
        var msg = $"[font size=10][color={color}]{proto.Description}[/color][/font]";

        chatManager.ChatMessageToOne(ChatChannel.Emotes, msg, msg, EntityUid.Invalid, false,
            comp.Session.Channel);
    }
}
