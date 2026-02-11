using System.Linq;
using Content.Shared._White.Bark;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._White.Bark;


public sealed class BarkPreviewSystem : EntitySystem
{
    [Dependency] private readonly BarkSystem _barkSystem = default!;
    [Dependency] private readonly AudioSystem _sharedAudio = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private Queue<(BarkData, SoundSpecifier)> _barks = new();
    private (BarkData, SoundSpecifier)? _currentBark;
    private float _barkTime;

    public void PlayGlobal(ProtoId<BarkVoicePrototype> protoId, string text, BarkPercentageApplyData? data = null)
    {
        if (!_prototypeManager.TryIndex(protoId, out var prototype))
            return;

        PlayGlobal(prototype, text, data);
    }

    public void PlayGlobal(BarkVoicePrototype prototype, string text, BarkPercentageApplyData? data = null)
    {
        var voiceData = BarkVoiceData.WithClampingValue(prototype.BarkSound, prototype.ClampData, data ?? BarkPercentageApplyData.Default);
        PlayGlobal(voiceData, _barkSystem.GenBarkData(voiceData, text, false));
    }

    public void PlayGlobal(BarkVoiceData barkVoiceData, List<BarkData> barks)
    {
        _barks = new(barks.Select(p => (p, barkVoiceData.BarkSound)));
        _barkTime = 0;
        _currentBark = null;
    }

    public override void Update(float frameTime)
    {
        if (!_gameTiming.IsFirstTimePredicted)
            return;

        if(_playerManager.LocalSession is null)
            return;

        if (_currentBark is null)
        {
            if (!_barks.TryDequeue(out var barkData))
            {
                _barkTime = 0;
                return;
            }

            _currentBark = barkData;
        }

        if (_currentBark.Value.Item1.Pause <= _barkTime)
        {
            _barkTime = 0;

            if (!_currentBark.Value.Item1.Enabled)
            {
                _currentBark = null;
                return;
            }

            _sharedAudio
                .PlayGlobal(
                    _currentBark.Value.Item2,
                    _playerManager.LocalSession,
                    new AudioParams()
                        .WithPitchScale(_currentBark.Value.Item1.Pitch)
                        .WithVolume(_currentBark.Value.Item1.Volume));
            _currentBark = null;
        }
        else
        {
            _barkTime += frameTime;
        }
    }
}
