using Content.Server.Psionics.Abilities;
using Content.Server.Chat.Systems;
using Content.Server.NPC.Events;
using Content.Server.NPC.Systems;
using Content.Server.NPC.Prototypes;
using Content.Server.Radio.Components;
using Content.Server.Radio.EntitySystems;
using Content.Server.StationEvents.Events;
using Content.Shared.Interaction;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Nyanotrasen.Research.SophicScribe;

public sealed partial class SophicScribeSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly RadioSystem _radioSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly NPCConversationSystem _conversationSystem = default!;
    protected ISawmill Sawmill = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_glimmerSystem.GlimmerOutput == 0)
            return; // yes, return. Glimmer value is global.

        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<SophicScribeComponent>();
        while (query.MoveNext(out var scribe, out var scribeComponent))
        {
            if (curTime < scribeComponent.NextAnnounceTime)
                continue;

            if (!TryComp<IntrinsicRadioTransmitterComponent>(scribe, out var radio))
                continue;

            var message = Loc.GetString("glimmer-report", ("level", (int) Math.Round(_glimmerSystem.GlimmerOutput)));
            var channel = _prototypeManager.Index<RadioChannelPrototype>("Science");
            _radioSystem.SendRadioMessage(scribe, message, channel, scribe);

            scribeComponent.NextAnnounceTime = curTime + scribeComponent.AnnounceInterval;
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SophicScribeComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<GlimmerEventEndedEvent>(OnGlimmerEventEnded);
        SubscribeLocalEvent<SophicScribeComponent, NPCConversationGetGlimmerEvent>(OnGetGlimmer);
    }

    private void OnGetGlimmer(EntityUid uid, SophicScribeComponent component, NPCConversationGetGlimmerEvent args)
    {
        if (args.Text == null)
        {
            Sawmill.Error($"{uid} heard a glimmer reading prompt but has no text for it");
            return;
        }

        var tier = _glimmerSystem.GetGlimmerTier() switch
        {
            GlimmerTier.Minimal => Loc.GetString("glimmer-reading-minimal"),
            GlimmerTier.Low => Loc.GetString("glimmer-reading-low"),
            GlimmerTier.Moderate => Loc.GetString("glimmer-reading-moderate"),
            GlimmerTier.High => Loc.GetString("glimmer-reading-high"),
            GlimmerTier.Dangerous => Loc.GetString("glimmer-reading-dangerous"),
            _ => Loc.GetString("glimmer-reading-critical"),
        };

        var glimmerReadingText = Loc.GetString(args.Text,
            ("glimmer", (int) Math.Round(_glimmerSystem.GlimmerOutput)), ("tier", tier));

        var response = new NPCResponse(glimmerReadingText);
        _conversationSystem.QueueResponse(uid, response);
    }

    private void OnInteractHand(EntityUid uid, SophicScribeComponent component, InteractHandEvent args)
    {
        //TODO: the update function should be removed eventually too.
        if (_timing.CurTime < component.StateTime)
            return;

        component.StateTime = _timing.CurTime + component.StateCD;

        _chat.TrySendInGameICMessage(uid, Loc.GetString("glimmer-report", ("level", (int) Math.Round(_glimmerSystem.GlimmerOutput))), InGameICChatType.Speak, true);
    }

    private void OnGlimmerEventEnded(GlimmerEventEndedEvent args)
    {
        var query = EntityQueryEnumerator<SophicScribeComponent>();
        while (query.MoveNext(out var scribe, out _))
        {
            if (!TryComp<IntrinsicRadioTransmitterComponent>(scribe, out var radio)) return;

            // mind entities when...
            var speaker = scribe;
            if (TryComp<MindSwappedComponent>(scribe, out var swapped))
            {
                speaker = swapped.OriginalEntity;
            }

            var message = Loc.GetString(args.Message, ("decrease", args.GlimmerBurned), ("level", (int) Math.Round(_glimmerSystem.GlimmerOutput)));
            var channel = _prototypeManager.Index<RadioChannelPrototype>("Common");
            _radioSystem.SendRadioMessage(speaker, message, channel, speaker);
        }
    }
    public sealed partial class NPCConversationGetGlimmerEvent : NPCConversationEvent
    {
        [DataField]
        public string? Text;
    }
}
