<<<<<<< HEAD
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
using Content.Server.Backmen.Blob.Components;
=======
using System.Numerics;
using Content.Server.Backmen.Blob.Components;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Popups;
using Content.Server.Radio.Components;
using Content.Server.Radio.EntitySystems;
using Content.Shared.Backmen.Blob;
using Content.Shared.Backmen.Blob.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
<<<<<<< HEAD
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
using Robust.Shared.Network;
using Robust.Shared.Player;
=======
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
using Robust.Shared.Prototypes;

namespace Content.Server.Backmen.Blob;

<<<<<<< HEAD
public sealed class BlobMobSystem : EntitySystem
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
public sealed class EntitySpeakPrivateTransformEvent(
    ICommonSession targetSession,
    ChatChannel chatChannel,
    EntityUid source,
    string message,
    string wrappedMessage,
    string? originalMessage,
    NetUserId? author,
    ChatSystem.ICChatRecipientData data)
    : EntityEventArgs
{
    public ICommonSession TargetSession { get; } = targetSession;
    public ChatChannel ChatChannel { get; set; } = chatChannel;
    public EntityUid Source { get; } = source;
    public string Message { get; set; } = message;
    public string WrappedMessage { get; set; } = wrappedMessage;
    public string? OriginalMessage { get; } = originalMessage;
    public NetUserId? Author { get; } = author;
    public ChatSystem.ICChatRecipientData Data { get; } = data;
}

public sealed class BlobMobSystem : EntitySystem
=======
public sealed class EntitySpeakPrivateTransformEvent(
    ICommonSession targetSession,
    ChatChannel chatChannel,
    EntityUid source,
    string message,
    string wrappedMessage,
    string? originalMessage,
    NetUserId? author,
    ChatSystem.ICChatRecipientData data)
    : EntityEventArgs
{
    public ICommonSession TargetSession { get; } = targetSession;
    public ChatChannel ChatChannel { get; set; } = chatChannel;
    public EntityUid Source { get; } = source;
    public string Message { get; set; } = message;
    public string WrappedMessage { get; set; } = wrappedMessage;
    public string? OriginalMessage { get; } = originalMessage;
    public NetUserId? Author { get; } = author;
    public ChatSystem.ICChatRecipientData Data { get; } = data;
}

public sealed class BlobMobSystem : SharedBlobMobSystem
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
<<<<<<< HEAD
    [Dependency] private readonly PopupSystem _popupSystem = default!;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    [Dependency] private readonly SharedChatSystem _chatSystem = default!;
=======

    [Dependency] private readonly SharedChatSystem _chatSystem = default!;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
    //[Dependency] private readonly SmokeSystem _smokeSystem = default!;
    //[Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly RadioSystem _radioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobMobComponent, BlobMobGetPulseEvent>(OnPulsed);

        SubscribeLocalEvent<BlobSpeakComponent, EntitySpokeEvent>(OnSpoke, before: new []{ typeof(RadioSystem) });
        SubscribeLocalEvent<BlobSpeakComponent, ComponentStartup>(OnSpokeAdd);
        SubscribeLocalEvent<BlobSpeakComponent, ComponentShutdown>(OnSpokeRemove);
        SubscribeLocalEvent<BlobSpeakComponent, TransformSpeakerNameEvent>(OnSpokeName);
        SubscribeLocalEvent<BlobSpeakComponent, SpeakAttemptEvent>(OnSpokeCan, after: new []{ typeof(SpeechSystem) });
        //SubscribeLocalEvent<SmokeOnTriggerComponent, TriggerEvent>(HandleSmokeTrigger);
    }

    private void OnSpokeName(Entity<BlobSpeakComponent> ent, ref TransformSpeakerNameEvent args)
    {
        args.Name = "Блоб";

    }

    private void OnSpokeCan(Entity<BlobSpeakComponent> ent, ref SpeakAttemptEvent args)
    {
        args.Uncancel();
    }

    private void OnSpokeRemove(Entity<BlobSpeakComponent> ent, ref ComponentShutdown args)
    {
        var radio = EnsureComp<ActiveRadioComponent>(ent);
        radio.Channels.Remove(ent.Comp.Channel);
        var snd = EnsureComp<IntrinsicRadioTransmitterComponent>(ent);
        snd.Channels.Remove(ent.Comp.Channel);
    }

    private void OnSpokeAdd(Entity<BlobSpeakComponent> ent, ref ComponentStartup args)
    {
        EnsureComp<IntrinsicRadioReceiverComponent>(ent);
        var radio = EnsureComp<ActiveRadioComponent>(ent);
        radio.Channels.Add(ent.Comp.Channel);
        var snd = EnsureComp<IntrinsicRadioTransmitterComponent>(ent);
        snd.Channels.Add(ent.Comp.Channel);
    }


    private void OnSpoke(Entity<BlobSpeakComponent> ent, ref EntitySpokeEvent args)
    {
        if (args.Channel == null)
            args.Channel = _prototypeManager.Index(ent.Comp.Channel);

        if (!TryComp<IntrinsicRadioTransmitterComponent>(ent, out var component) ||
            !component.Channels.Contains(args.Channel.ID) ||
            args.Channel.ID != ent.Comp.Channel)
        {
            return;
        }

        if (TryComp<BlobObserverComponent>(ent, out var blobObserverComponent) && blobObserverComponent.Core.HasValue)
        {
            _radioSystem.SendRadioMessage(blobObserverComponent.Core.Value, args.OriginalMessage, args.Channel, blobObserverComponent.Core.Value);
        }
        else
        {
            _radioSystem.SendRadioMessage(ent, args.OriginalMessage, args.Channel, ent);
        }

        args.Channel = null; // prevent duplicate messages from other listeners.
    }


    private void OnPulsed(EntityUid uid, BlobMobComponent component, BlobMobGetPulseEvent args)
    {
        _damageableSystem.TryChangeDamage(uid, component.HealthOfPulse);
    }


}
