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

    private const float AmbientMusicFadeTime = 10f;
    private static float _volumeSlider;
    private EntityUid? _ambientMusicStream;
    private AmbientMusicPrototype? _musicProto;

    // Need to keep track of the last biome we were in to re-play its music when we're out of combat mode
    private SpaceBiomePrototype? _lastBiome;

    // Because ToggleCombatActionEvent triggers 7 times for some reason, we keep track of the last state as to not play combat music 7 times in a row.
    private bool _lastCombatState = false;

    private List<AmbientMusicPrototype>? _musicTracks;

    /// <summary>
    /// If we find a better ambient music proto can we interrupt this one.
    /// </summary>
    private bool _interruptable;

    /// <summary>
    /// Track what ambient sounds we've played. This is so they all get played an even
    /// number of times.
    /// When we get to the end of the list we'll re-shuffle
    /// </summary>
    private readonly Dictionary<string, List<ResPath>> _ambientSounds = new();

    private ISawmill _sawmill = default!;

    private void InitializeAmbientMusic()
    {
        SubscribeNetworkEvent<SpaceBiomeSwapMessage>(OnBiomeChange);
        SubscribeLocalEvent<ToggleCombatActionEvent>(OnCombatModeToggle);

        Subs.CVar(_configManager, CCVars.AmbientMusicVolume, AmbienceCVarChanged, true);
        _sawmill = IoCManager.Resolve<ILogManager>().GetSawmill("audio.ambience");

        // Setup ambient tracks
        _musicTracks = GetTracks();

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnProtoReload);
        _state.OnStateChanged += OnStateChange;
        // On round end summary OR lobby cut audio.
        SubscribeNetworkEvent<RoundEndMessageEvent>(OnRoundEndMessage);
    }


    //FADEIN AND FADEOUT MIGHT INTERFERE?
    //NEED TO MAKE THIS TRIGGER WHEN U SPAWN IN
    //ISSUE: WON'T REPLAY MUSIC AFTER IT ENDS. NEED TO FIX THAT
    //MAYBE MOVE THE PLAYING PART TO A FUNCTION? CALL THAT WHEN THE SONG IS OVER?
    //ISSUE: WON'T PLAY MUSIC WHEN YOU REJOIN BECAUSE YOU ARENT ENTERING A BIOME
    //ISSUE: FOR COMBAT MUSIC WE NEED TO **KILL** THE STREAM, NOT FADE OUT!
    private void OnBiomeChange(SpaceBiomeSwapMessage ev)
    {
        _sawmill.Debug($"went to biome {ev.Biome}");

        SpaceBiomePrototype biome = _protMan.Index<SpaceBiomePrototype>(ev.Biome); //get the biome prototype
        _lastBiome = biome; //save biome in case we are in combat mode

        _sawmill.Debug($"last biome is {_lastBiome.ID}");

        if (_combatModeSystem.IsInCombatMode()) //we don't want to change music if we are in combat mode right now
            return;

        FadeOut(_ambientMusicStream);
        float volume = 10; //default value in case we use the fallback track

        if (_musicTracks == null)
            return;

        _musicProto = null;

        foreach (var ambient in _musicTracks)
        {
            //IF THIS DOESNT FIND ANYTHING WE NEED TO PLAY THE FALLBACK TRACK!
            if (biome.ID == ambient.ID) //if we find the biome that's matching the ambient's ID, we play that track!
            {
                _sawmill.Debug($"found biome match: {biome.ID} == {ambient.ID}");
                _musicProto = ambient;
                _sawmill.Debug($"music proto is now {_musicProto.ID}");
                volume = ambient.Sound.Params.Volume;
                break;
            }
        }

        if (_musicProto == null) //THIS SHOULD CHANGE TO THE FALLBACK TRACK!!!!
        {
            _musicProto = _proto.Index<AmbientMusicPrototype>("fallback");
        }

        SoundCollectionPrototype soundcol = _proto.Index<SoundCollectionPrototype>(_musicProto.ID); //THIS IS WHAT ERRORS!

        string path = _random.Pick(soundcol.PickFiles).ToString(); // THIS WILL PICK A RANDOM SOUND. WE MAY WANT TO SPECIFY ONE INSTEAD!!

        _sawmill.Debug($"SOUND PATH: {path}");

        var strim = _audio.PlayGlobal(
        path,
        Filter.Local(),
        false,
        AudioParams.Default.WithVolume(_musicProto.Sound.Params.Volume + _volumeSlider))!;

        _ambientMusicStream = strim.Value.Entity; //THIS SHOULD PLAY THE TRACK!!

        FadeIn(_ambientMusicStream, strim.Value.Component, AmbientMusicFadeTime);
    }

    private void OnCombatModeToggle(ToggleCombatActionEvent ev) //CombatModeOnMessage ev, MUST INCLUDE FACTION!!!
    {
        bool currentCombatState = _combatModeSystem.IsInCombatMode();
        //EXPLANATION: because ToggleCombatActionEvent triggers 7 times for some reason, we ignore repeated calls if the state is ON>ON>ON>ON. Only ON>OFF or OFF>ON.
        if (currentCombatState == _lastCombatState)
            return;

        _lastCombatState = currentCombatState; //update last state if we are successful!

        if (currentCombatState) //true = we toggled combat ON. 
        {
            FadeOut(_ambientMusicStream);
            _musicProto = _proto.Index<AmbientMusicPrototype>("combatmode");
            SoundCollectionPrototype soundcol = _proto.Index<SoundCollectionPrototype>(_musicProto.ID); //THIS IS WHAT ERRORS!

            string path = _random.Pick(soundcol.PickFiles).ToString(); // THIS WILL PICK A RANDOM SOUND. WE MAY WANT TO SPECIFY ONE INSTEAD!!

            _sawmill.Debug($"SOUND PATH: {path}");

            var strim = _audio.PlayGlobal(
            path,
            Filter.Local(),
            false,
            AudioParams.Default.WithVolume(_musicProto.Sound.Params.Volume + _volumeSlider))!;

            _ambientMusicStream = strim.Value.Entity; //THIS SHOULD PLAY THE TRACK!!

            FadeIn(_ambientMusicStream, strim.Value.Component, 1f);
        }
        else                    //false = we toggled combat OFF
        {
            FadeOut(_ambientMusicStream);

            if (_lastBiome == null) //this should never happen still
                return;

            _musicProto = _proto.Index<AmbientMusicPrototype>(_lastBiome.ID);

            SoundCollectionPrototype soundcol = _proto.Index<SoundCollectionPrototype>(_musicProto.ID); //THIS IS WHAT ERRORS!

            string path = _random.Pick(soundcol.PickFiles).ToString(); // THIS WILL PICK A RANDOM SOUND. WE MAY WANT TO SPECIFY ONE INSTEAD!!

            _sawmill.Debug($"SOUND PATH: {path}");

            var strim = _audio.PlayGlobal(
            path,
            Filter.Local(),
            false,
            AudioParams.Default.WithVolume(_musicProto.Sound.Params.Volume + _volumeSlider))!;

            FadeIn(_ambientMusicStream, strim.Value.Component, AmbientMusicFadeTime);
        }

        /*
        forceStop(ambientMusicStream)

        switch (CombatModeOnMessage.Faction)
            case NCWL
                AmbientMusicStream = PlayGlobal (ncwl track)
            case DSM
                ....
            ...
            case default
                AmbientMusicStream = PlayGlobal (awakening.ogg)

        activate(ambientMusicStream)

        */
    }

    //TODO: ForceAmbientMusic
    //TODO: DisableAmbientMusic
    //TODO: EnableAmbientMusic

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


    ///<summary>
    /// This function counts all ambient musics we have in ambient_music.yml and returns that list.
    /// This should only run once at roundstart.
    ///</summary>
    /*private void SetupAmbientSounds()
    {
        _ambientSounds.Clear();
        foreach (var ambience in _proto.EnumeratePrototypes<AmbientMusicPrototype>())
        {
            _sawmill.Debug($"logged ambient sound {ambience.ID}");
            var tracks = _ambientSounds.GetOrNew(ambience.ID);
            RefreshTracks(ambience.Sound, tracks, null);
            _random.Shuffle(tracks);
        }
    }*/

    private void OnRoundEndMessage(RoundEndMessageEvent ev)
    {
        // If scoreboard shows then just stop the music
        _ambientMusicStream = _audio.Stop(_ambientMusicStream);
    }

    /*private void RefreshTracks(SoundSpecifier sound, List<ResPath> tracks, ResPath? lastPlayed)
    {
        DebugTools.Assert(tracks.Count == 0);

        switch (sound)
        {
            case SoundCollectionSpecifier collection:
                if (collection.Collection == null)
                    break;

                var slothCud = _proto.Index<SoundCollectionPrototype>(collection.Collection);
                tracks.AddRange(slothCud.PickFiles);
                break;
            case SoundPathSpecifier path:
                tracks.Add(path.Path);
                break;
        }

        // Just so the same track doesn't play twice
        if (tracks.Count > 1 && tracks[^1] == lastPlayed)
        {
            (tracks[0], tracks[^1]) = (tracks[^1], tracks[0]);
        }
    }*/
    ///<summary>
    /// This function runs every frame, and handles changing audio. This fucking sucks and I'm refactoring it. -.2
    ///</summary>
    /*private void UpdateAmbientMusic()
    {
        // Update still runs in lobby so just ignore it.
        if (_state.CurrentState is not GameplayState)
        {
            _ambientMusicStream = Audio.Stop(_ambientMusicStream);
            _musicProto = null;
            return;
        }

        bool? isDone = null;

        if (TryComp(_ambientMusicStream, out AudioComponent? audioComp))
        {
            isDone = !audioComp.Playing;
        }

        if (_interruptable)
        {
            var player = _player.LocalSession?.AttachedEntity;

            if (player == null || _musicProto == null || !_rules.IsTrue(player.Value, _proto.Index<RulesPrototype>(_musicProto.Rules)))
            {
                FadeOut(_ambientMusicStream, duration: AmbientMusicFadeTime);
                _musicProto = null;
                _interruptable = false;
                isDone = true;
            }
        }

        // Still running existing ambience
        if (isDone == false)
            return;

        // If ambience finished reset the CD (this also means if we have long ambience it won't clip)
        if (isDone == true)
        {
            // Also don't need to worry about rounding here as it doesn't affect the sim
            _nextAudio = _timing.CurTime + _random.Next(_minAmbienceTime, _maxAmbienceTime);
        }

        _ambientMusicStream = null;

        if (_nextAudio > _timing.CurTime)
            return;

        _musicProto = GetAmbience();

        if (_musicProto == null)
        {
            _interruptable = false;
            return;
        }
        _interruptable = _musicProto.Interruptable;
        var tracks = _ambientSounds[_musicProto.ID];
        //_sawmill.Debug($"tracks = {tracks.Count}");
        var track = tracks[^1];
        tracks.RemoveAt(tracks.Count - 1);

        var strim = _audio.PlayGlobal(
            track.ToString(),
            Filter.Local(),
            false,
            AudioParams.Default.WithVolume(_musicProto.Sound.Params.Volume + _volumeSlider))!;

        _ambientMusicStream = strim.Value.Entity;

        if (_musicProto.FadeIn)
        {
            FadeIn(_ambientMusicStream, strim.Value.Component, AmbientMusicFadeTime);
        }

        // Refresh the list
        if (tracks.Count == 0)
        {
            RefreshTracks(_musicProto.Sound, tracks, track);
        }
    }

    //<summary>
    // This gathers all the ambient music prototypes, sorts them by priority, and then checks in priority order of priority for the first which has its rule true.
    //</summary>
    private AmbientMusicPrototype? GetAmbience()
    {
        var player = _player.LocalEntity;

        if (player == null)
            return null;

        var ev = new PlayAmbientMusicEvent();
        RaiseLocalEvent(ref ev);

        if (ev.Cancelled)
            return null;

        var ambiences = _proto.EnumeratePrototypes<AmbientMusicPrototype>().ToList();
        ambiences.Sort((x, y) => y.Priority.CompareTo(x.Priority));

        foreach (var amb in ambiences)
        {
            if (!_rules.IsTrue(player.Value, _proto.Index<RulesPrototype>(amb.Rules)))
                continue;

            return amb;
        }

        _sawmill.Warning($"Unable to find fallback ambience track");
        return null;
    }*/

    /// <summary>
    /// Fades out the current ambient music temporarily.
    /// </summary>
    public void DisableAmbientMusic()
    {
        FadeOut(_ambientMusicStream);
        _ambientMusicStream = null;
    }
}
