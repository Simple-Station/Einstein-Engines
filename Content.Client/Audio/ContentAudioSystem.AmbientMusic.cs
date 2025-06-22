using System.ComponentModel.DataAnnotations;
using System.Linq;
using Content.Client.Gameplay;
using Content.Shared._Crescent.SpaceBiomes;
using Content.Shared.Audio;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Random;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Content.Client.CombatMode;
using Content.Shared.CombatMode;
using System.IO;
using Robust.Shared.Toolshed.Commands.Values;

namespace Content.Client.Audio;

public sealed partial class ContentAudioSystem
{
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IStateManager _state = default!;
    [Dependency] private readonly RulesSystem _rules = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly CombatModeSystem _combatModeSystem = default!; //CLIENT ONE. WHY ARE THERE 3???
    [Dependency] private readonly IPrototypeManager _protMan = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    private static float _volumeSlider;
    private EntityUid? _ambientMusicStream;
    private AmbientMusicPrototype? _musicProto;

    // Need to keep track of the last biome we were in to re-play its music when we're out of combat mode
    private SpaceBiomePrototype? _lastBiome;

    // Need to keep track of how long the current track's been playing for to know when to play the next
    private TimeSpan _trackTimeRemaining = TimeSpan.MaxValue;

    private List<AmbientMusicPrototype>? _musicTracks;

    private ISawmill _sawmill = default!;

    private void InitializeAmbientMusic()
    {
        SubscribeNetworkEvent<SpaceBiomeSwapMessage>(OnBiomeChange);
        SubscribeLocalEvent<ToggleCombatActionEvent>(OnCombatModeToggle);

        Subs.CVar(_configManager, CCVars.AmbientMusicVolume, AmbienceCVarChanged, true);
        _sawmill = IoCManager.Resolve<ILogManager>().GetSawmill("audio.ambience");

        // Setup tracks to pull from. Runs once.
        _musicTracks = GetTracks();

        // Handle music if the player rejoins or crashes.
        // if (_player.LocalSession != null)
        // {
        //     _entMan.TryGetComponent<SpaceBiomeTrackerComponent>(_player.LocalSession.AttachedEntity, out var comp);
        //     if (comp != null)
        //         _lastBiome = _proto.Index<SpaceBiomePrototype>(comp.Biome);
        // }

        //SubscribeNetworkEvent<PlayerBeforeSpawnEvent>(OnPlayerRejoin);

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnProtoReload);
        _state.OnStateChanged += OnStateChange;
        // On round end summary OR lobby cut audio.
        SubscribeNetworkEvent<RoundEndMessageEvent>(OnRoundEndMessage);
    }

    // private void OnPlayerRejoin(PlayerBeforeSpawnEvent ev)
    // {
    //     if (_lastBiome == null) //this should never happen still
    //         return;

    //     _musicProto = _proto.Index<AmbientMusicPrototype>(_lastBiome.ID);

    //     SoundCollectionPrototype soundcol = _proto.Index<SoundCollectionPrototype>(_musicProto.ID);

    //     string path = _random.Pick(soundcol.PickFiles).ToString(); // THIS WILL PICK A RANDOM SOUND. WE MAY WANT TO SPECIFY ONE INSTEAD!!

    //     PlayMusicTrack(path, _musicProto.Sound.Params.Volume, false);
    // }

    //NEED TO MAKE THIS TRIGGER WHEN U SPAWN IN
    //ISSUE: WON'T REPLAY MUSIC AFTER IT ENDS. NEED TO FIX THAT
    //MAYBE MOVE THE PLAYING PART TO A FUNCTION? CALL THAT WHEN THE SONG IS OVER?
    //ISSUE: WON'T PLAY MUSIC WHEN YOU REJOIN BECAUSE YOU ARENT ENTERING A BIOME
    //ISSUE: WHEN COMBAT MODE SPAMMING IT GETS CONFUSED AND KEEPS PLAYING MUSIC
    private void OnBiomeChange(SpaceBiomeSwapMessage ev)
    {
        //_sawmill.Debug($"went to biome {ev.Biome}");

        SpaceBiomePrototype biome = _protMan.Index<SpaceBiomePrototype>(ev.Biome); //get the biome prototype
        _lastBiome = biome; //save biome in case we are in combat mode

        if (_combatModeSystem.IsInCombatMode()) //we don't want to change music if we are in combat mode right now
            return;

        FadeOut(_ambientMusicStream);

        if (_musicTracks == null)
            return;

        _musicProto = null;

        foreach (var ambient in _musicTracks)
        {
            if (biome.ID == ambient.ID) //if we find the biome that's matching the ambient's ID, we play that track!
            {
                //_sawmill.Debug($"found biome match: {biome.ID} == {ambient.ID}");
                _musicProto = ambient;
                break;
            }
        }

        if (_musicProto == null) //if we don't find any, we play the default track.
        {
            _musicProto = _proto.Index<AmbientMusicPrototype>("default");
            _lastBiome = _proto.Index<SpaceBiomePrototype>("default");
        }

        SoundCollectionPrototype soundcol = _proto.Index<SoundCollectionPrototype>(_musicProto.ID);

        string path = _random.Pick(soundcol.PickFiles).ToString(); // THIS WILL PICK A RANDOM SOUND. WE MAY WANT TO SPECIFY ONE INSTEAD!!

        PlayMusicTrack(path, _musicProto.Sound.Params.Volume, true);
    }

    //TODO: include faction
    private void OnCombatModeToggle(ToggleCombatActionEvent ev) //CombatModeOnMessage ev, MUST INCLUDE FACTION!!!
    {
        if (!_timing.IsFirstTimePredicted == true) //needed, because combat mode is predicted, and triggers 7 times otherwise.
            return;

        bool currentCombatState = _combatModeSystem.IsInCombatMode();

        _audio.Stop(_ambientMusicStream);

        if (currentCombatState) //true = we toggled combat ON. 
        {
            _musicProto = _proto.Index<AmbientMusicPrototype>("combatmode");
            SoundCollectionPrototype soundcol = _proto.Index<SoundCollectionPrototype>(_musicProto.ID); //THIS IS WHAT ERRORS!

            string path = _random.Pick(soundcol.PickFiles).ToString(); // THIS WILL PICK A RANDOM SOUND. WE MAY WANT TO SPECIFY ONE INSTEAD!!

            PlayMusicTrack(path, _musicProto.Sound.Params.Volume, false);
        }
        else                    //false = we toggled combat OFF
        {
            _sawmill.Debug("TURNING COMBAT MODE OFF");
            if (_lastBiome == null) //this should never happen still
            {
                _sawmill.Debug("1: LASTBIOME IS NULL...");
                if (_player.LocalSession != null) //THIS LITERALLY CANNOT BE NULL!!
                {
                    _sawmill.Debug("2: PLAYER LOCALSESSION IS NOT NULL...");
                    _entMan.TryGetComponent<SpaceBiomeTrackerComponent>(_player.LocalSession.AttachedEntity, out var comp);
                    _sawmill.Debug("3. TRYING TO GRAB THE COMPONENT...");
                    if (comp != null)
                    {
                        _lastBiome = _proto.Index<SpaceBiomePrototype>(comp.Biome);
                        _sawmill.Debug($"4. GRABBED COMPONENT {comp.Biome}");
                    }
                }
            }

            if (_lastBiome == null)
                return;

            _musicProto = _proto.Index<AmbientMusicPrototype>(_lastBiome.ID); //THIS CAN FUCK UP! BECAUSE THE ID MIGHT NOT HAVE MUSIC AND BE A FALLBACK!

            SoundCollectionPrototype soundcol = _proto.Index<SoundCollectionPrototype>(_musicProto.ID); //THIS IS WHAT ERRORS!

            string path = _random.Pick(soundcol.PickFiles).ToString(); // THIS WILL PICK A RANDOM SOUND. WE MAY WANT TO SPECIFY ONE INSTEAD!!

            PlayMusicTrack(path, _musicProto.Sound.Params.Volume, false);
        }
    }

    //TODO: ForceAmbientMusic
    //TODO: DisableAmbientMusic
    //TODO: EnableAmbientMusic

    private void PlayMusicTrack(string path, float volume, bool fadeIn)
    {

        _sawmill.Debug($"NOW PLAYING: {path}");

        var strim = _audio.PlayGlobal(
            path,
            Filter.Local(),
            false,
            AudioParams.Default.WithVolume(volume + _volumeSlider))!;

        _ambientMusicStream = strim.Value.Entity;

        if (fadeIn)
            FadeIn(_ambientMusicStream, strim.Value.Component, 2f);
    }

    private List<AmbientMusicPrototype> GetTracks()
    {
        List<AmbientMusicPrototype> musictracks = new List<AmbientMusicPrototype>();

        bool fallback = true;
        foreach (var ambience in _proto.EnumeratePrototypes<AmbientMusicPrototype>())
        {
            _sawmill.Debug($"logged ambient sound {ambience.ID}");
            musictracks.Add(ambience);
            fallback = false;
        }

        if (fallback) //if we somehow FOUND NO MUSIC TRACKS
        {
            _sawmill.Debug($"NO MUSIC FOUND, NEED FALLBACK!!!");
        }

        return musictracks;
    }
    private void AmbienceCVarChanged(float obj)
    {
        _volumeSlider = SharedAudioSystem.GainToVolume(obj);

        if (_ambientMusicStream != null && _musicProto != null)
        {
            _audio.SetVolume(_ambientMusicStream, _musicProto.Sound.Params.Volume + _volumeSlider);
        }
    }

    private void ShutdownAmbientMusic()
    {
        _state.OnStateChanged -= OnStateChange;
        _ambientMusicStream = _audio.Stop(_ambientMusicStream);
    }

    private void OnProtoReload(PrototypesReloadedEventArgs obj)
    {
        if (obj.WasModified<AmbientMusicPrototype>())
            _musicTracks = GetTracks();
    }
    ///<summary>
    /// This function handles the change from lobby to gameplay.
    ///</summary>
    private void OnStateChange(StateChangedEventArgs obj)
    {
        if (obj.NewState is not GameplayState)
            return;
    }

    private void OnRoundEndMessage(RoundEndMessageEvent ev)
    {
        if (_ambientMusicStream == null)
        {
            _sawmill.Debug("AMBIENT MUSIC STREAM WAS NULL? FROM OnRoundEndMessage()");
            return;
        }
        // If scoreboard shows then just stop the music
        _ambientMusicStream = _audio.Stop(_ambientMusicStream);
    }

    public void DisableAmbientMusic()
    {
        if (_ambientMusicStream == null)
        {
            _sawmill.Debug("AMBIENT MUSIC STREAM WAS NULL? FROM DisableAmbientMusic()");
            return;
        }
        FadeOut(_ambientMusicStream);
        _ambientMusicStream = null;
    }
}
