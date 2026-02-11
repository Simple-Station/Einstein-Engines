using System.Linq;
using Content.Shared._White.Bark.Components;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._White.Bark.Systems;


public abstract class SharedBarkSystem : EntitySystem
{
    private static readonly char[] LongPauseChars = ['.', ',', '?', '!',];
    private static readonly char[] SkipChars = [' ', '\n', '\r', '\t',];

    private static readonly char[] Soglasnoy =
        ['Б', 'В', 'Г', 'Д', 'Ж', 'З', 'Й', 'К', 'Л', 'М', 'Н', 'П', 'Р', 'С', 'Т', 'Ф', 'Х', 'Ц', 'Ч', 'Ш', 'Щ',];

    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ApplyBarkProtoComponent, ComponentInit>(OnApplyBarkInit);
    }

    private void OnApplyBarkInit(Entity<ApplyBarkProtoComponent> ent, ref ComponentInit args)
    {
        ApplyBark(ent.Owner, ent.Comp.VoiceProto, ent.Comp.PercentageApplyData);
        RemComp(ent.Owner, ent.Comp);
    }

    public List<BarkVoicePrototype> GetVoiceList(
        HumanoidCharacterProfile profile,
        JobPrototype? job = null,
        ProtoId<BarkListPrototype>? id = null
        )
    {
        var listRaw = _prototypeManager.Index(id ?? "default");
        var list = new List<BarkVoicePrototype>();
        foreach (var (voice, requirements) in listRaw.VoiceList)
        {
            if(!_prototypeManager.TryIndex(voice, out var prototype))
                continue;

            var isValid = true;

            foreach (var requirement in requirements)
            {
                if (requirement.IsValid(
                    job ?? default!,
                    profile,
                    new Dictionary<string, TimeSpan>(),
                    false,
                    prototype,
                    EntityManager,
                    _prototypeManager,
                    _cfg,
                    out var reason) == !requirement.Inverted)
                    continue;

                isValid = false;
                break;
            }

            if (!isValid)
                continue;

            list.Add(prototype);
        }

        return list;
    }

    public void ApplyBark(EntityUid uid, ProtoId<BarkVoicePrototype> protoId, BarkPercentageApplyData? data = null)
    {
        if (!_prototypeManager.TryIndex(protoId, out var prototype))
            return;

        ApplyBark(uid, prototype.BarkSound, prototype.ClampData, data);
    }

    public void ApplyBark(EntityUid uid, SoundSpecifier barkSound, BarkClampData clampData, BarkPercentageApplyData? data = null)
    {
        var voiceData = BarkVoiceData.WithClampingValue(
            barkSound,
            clampData,
            data ?? BarkPercentageApplyData.Default);

        var wasBarkComp = HasComp<BarkComponent>(uid);
        var comp = EnsureComp<BarkComponent>(uid);
        comp.VoiceData = voiceData;

        if (wasBarkComp)
            Dirty(uid, comp);
    }

    public List<BarkData> GenBarkData(BarkVoiceData data, string text, bool isWhisper) =>
        text.Select(currChar => GenBarkData(data, currChar, isWhisper)).ToList();

    public BarkData GenBarkData(BarkVoiceData data, char currChar, bool isWhisper)
    {
        var currBark = new BarkData(
            data.PitchAverage,
            data.VolumeAverage,
            data.PauseAverage);

        if (SkipChars.Contains(currChar))
            currBark.Enabled = false;

        if (LongPauseChars.Contains(currChar))
        {
            currBark.Pause *= 1.2f;
            currBark.Enabled = false;
        }

        if (isWhisper)
            currBark.Volume -= SharedAudioSystem.GainToVolume(4f);

        if (Soglasnoy.Contains(currChar))
        {
            currBark.Pitch -= 0.2f;
            currBark.Volume -= SharedAudioSystem.GainToVolume(4f);
            currBark.Pause *= 0.8f;
        }

        currBark.Pitch += System.Random.Shared.NextFloat(-data.PitchVariance, data.PitchVariance);

        return currBark;
    }

    public void Bark(Entity<BarkComponent> entity, string text, bool isWhisper)
    {
        var ev = new TransformSpeakerBarkEvent(entity, entity.Comp.VoiceData);
        RaiseLocalEvent(entity, ev);
        Bark(entity, GenBarkData(ev.VoiceData, text, isWhisper));
    }

    public abstract void Bark(Entity<BarkComponent> entity, List<BarkData> barks);
}

public sealed class TransformSpeakerBarkEvent(EntityUid sender, BarkVoiceData voiceData) : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;

    public EntityUid Sender = sender;
    public BarkVoiceData VoiceData = voiceData;
}
