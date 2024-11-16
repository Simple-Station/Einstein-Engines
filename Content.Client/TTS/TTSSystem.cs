using Content.Client.Audio;
using Content.Shared.CCVar;
using Content.Shared.TTS;
using Robust.Client.Audio;
using Robust.Client.ResourceManagement;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Utility;

namespace Content.Client.TTS;

/// Plays TTS audio in-world
// ReSharper disable once InconsistentNaming
public sealed class TTSSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IResourceManager _res = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    private ISawmill _sawmill = default!;
    private readonly MemoryContentRoot _contentRoot = new();
    private static readonly ResPath Prefix = ResPath.Root ;

    /// Reducing the volume of the TTS when whispering. Will be converted to logarithm.
    private const float WhisperFade = 4f;

    /// The volume at which the TTS sound will not be heard.
    private const float MinimalVolume = -10f;

    private float _volume = CCVars.TTSVolume.DefaultValue;
    private int _fileIdx = 0;

    public override void Initialize()
    {
        _sawmill = Logger.GetSawmill("tts");
        _res.AddRoot(Prefix, _contentRoot);
        OnTtsVolumeChanged(_cfg.GetCVar(CCVars.TTSVolume));
        _cfg.OnValueChanged(CCVars.TTSVolume, OnTtsVolumeChanged, true);
        OnTtsUnknownVolumeChanged(_cfg.GetCVar(CCVars.TTSUnknownVolume));
        _cfg.OnValueChanged(CCVars.TTSUnknownVolume, OnTtsUnknownVolumeChanged, true);
        SubscribeNetworkEvent<PlayTTSEvent>(OnPlayTTS);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _cfg.UnsubValueChanged(CCVars.TTSVolume, OnTtsVolumeChanged);
        _cfg.UnsubValueChanged(CCVars.TTSUnknownVolume, OnTtsUnknownVolumeChanged);
        _contentRoot.Dispose();
    }

    public void RequestPreviewTTS(string voiceId)
    {
        RaiseNetworkEvent(new RequestPreviewTTSEvent(voiceId));
    }

    private void OnTtsVolumeChanged(float volume)
    {
        _volume = volume;
    }

    private void OnTtsUnknownVolumeChanged(float volume)
    {
        _volume = volume;
    }

    private void OnPlayTTS(PlayTTSEvent ev)
    {
        _sawmill.Verbose($"Playing TTS audio {ev.Data.Length} bytes from {ev.SourceUid} entity");

        var filePath = new ResPath($"{_fileIdx++}.wav");
        _contentRoot.AddOrUpdateFile(filePath, ev.Data);

        var audioResource = new AudioResource();
        audioResource.Load(IoCManager.Instance!, Prefix / filePath);

        var audioParams = AudioParams.Default
            .WithVolume(AdjustVolume(ev.IsWhisper))
            .WithMaxDistance(AdjustDistance(ev.IsWhisper));

        if (ev.SourceUid != null)
            _audio.PlayEntity(audioResource.AudioStream, GetEntity(ev.SourceUid.Value), audioParams);
        else
            _audio.PlayGlobal(audioResource.AudioStream, audioParams);

        _contentRoot.RemoveFile(filePath);
    }

    private float AdjustVolume(bool isWhisper)
    {
        var volume = MinimalVolume + SharedAudioSystem.GainToVolume(_volume);

        if (isWhisper)
            volume -= SharedAudioSystem.GainToVolume(WhisperFade);

        return volume;
    }

    private float AdjustDistance(bool isWhisper)
    {
        return isWhisper ? 5 : 10;
    }
}
