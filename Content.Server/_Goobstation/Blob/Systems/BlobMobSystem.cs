using Content.Server.Language;
using Content.Server.Chat.Systems;
using Content.Server.Radio;
using Content.Server.Radio.Components;
using Content.Server.Radio.EntitySystems;
using Content.Shared._Goobstation.Blob;
using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Speech;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Content.Shared.Language.Events;
using Content.Shared.Language.Components;
using Content.Shared._Shitmed.Targeting;

namespace Content.Server._Goobstation.Blob.Systems;

public sealed class BlobMobSystem : SharedBlobMobSystem
{
    [Dependency] private readonly LanguageSystem _language = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly RadioSystem _radioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobMobComponent, BlobMobGetPulseEvent>(OnPulsed);

        SubscribeLocalEvent<BlobSpeakComponent, DetermineEntityLanguagesEvent>(OnLanguageApply);
        SubscribeLocalEvent<BlobSpeakComponent, ComponentStartup>(OnSpokeAdd);
        SubscribeLocalEvent<BlobSpeakComponent, ComponentShutdown>(OnSpokeRemove);
        SubscribeLocalEvent<BlobSpeakComponent, TransformSpeakerNameEvent>(OnSpokeName);
        SubscribeLocalEvent<BlobSpeakComponent, SpeakAttemptEvent>(OnSpokeCan, after: new []{ typeof(SpeechSystem) });
        SubscribeLocalEvent<BlobSpeakComponent, EntitySpokeEvent>(OnSpoke, before: new []{ typeof(RadioSystem), typeof(HeadsetSystem) });
        SubscribeLocalEvent<BlobSpeakComponent, RadioReceiveEvent>(OnIntrinsicReceive);
    }

    private void OnIntrinsicReceive(Entity<BlobSpeakComponent> ent, ref RadioReceiveEvent args)
    {
        if (TryComp(ent, out ActorComponent? actor) && args.Channel.ID == ent.Comp.Channel)
        {
            var msg = new MsgChatMessage
            {
                Message = args.OriginalChatMsg
            };
            _netMan.ServerSendMessage(msg, actor.PlayerSession.Channel);
        }
    }

    private void OnSpoke(Entity<BlobSpeakComponent> ent, ref EntitySpokeEvent args)
    {
        if (args.Channel == null)
            return;
        _radioSystem.SendRadioMessage(ent, args.Message, ent.Comp.Channel, ent, language: args.Language);
    }

    private void OnLanguageApply(Entity<BlobSpeakComponent> ent, ref DetermineEntityLanguagesEvent args)
    {
        if (ent.Comp.LifeStage is
           ComponentLifeStage.Removing
           or ComponentLifeStage.Stopping
           or ComponentLifeStage.Stopped)
            return;

        args.SpokenLanguages.Clear();
        args.SpokenLanguages.Add(ent.Comp.Language);
        args.UnderstoodLanguages.Add(ent.Comp.Language);
    }

    private void OnSpokeName(Entity<BlobSpeakComponent> ent, ref TransformSpeakerNameEvent args)
    {
        if (!ent.Comp.OverrideName)
        {
            return;
        }
        args.VoiceName = Loc.GetString(ent.Comp.Name);
    }

    private void OnSpokeCan(Entity<BlobSpeakComponent> ent, ref SpeakAttemptEvent args)
    {
        if (HasComp<BlobCarrierComponent>(ent))
        {
            return;
        }
        args.Uncancel();
    }

    private void OnSpokeRemove(Entity<BlobSpeakComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        _language.UpdateEntityLanguages(ent.Owner);
        var radio = EnsureComp<ActiveRadioComponent>(ent);
        radio.Channels.Remove(ent.Comp.Channel);
    }

    private void OnSpokeAdd(Entity<BlobSpeakComponent> ent, ref ComponentStartup args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        var component = EnsureComp<LanguageSpeakerComponent>(ent);
        component.CurrentLanguage = ent.Comp.Language;
        _language.UpdateEntityLanguages(ent.Owner);

        var radio = EnsureComp<ActiveRadioComponent>(ent);
        radio.Channels.Add(ent.Comp.Channel);
    }

    private void OnPulsed(EntityUid uid, BlobMobComponent component, BlobMobGetPulseEvent args) =>
        _damageableSystem.TryChangeDamage(uid, component.HealthOfPulse, targetPart: TargetBodyPart.All);
}
