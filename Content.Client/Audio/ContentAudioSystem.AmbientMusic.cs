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
using Timer = Robust.Shared.Timing.Timer;
using Robust.Shared.Utility;
using Content.Client.CombatMode;
using Content.Shared.CombatMode;
using System.IO;
using Robust.Shared.Toolshed.Commands.Values;
using Content.Shared.Preferences;
using Content.Client.Lobby;
using System.Diagnostics;
using System.Threading;
using Robust.Shared.Timing;

namespace Content.Client.Audio;

/// <summary>
/// This handles playing ambient music over time, and combat music per faction.
/// </summary>
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
    [Dependency] private readonly IClientPreferencesManager _prefsManager = default!;

    private static float _volumeSlider;

    // This stores the music stream. It's used to start/stop the music on the fly.
    private EntityUid? _ambientMusicStream;

    // This stores the ambient music prototype to be played,.
    private AmbientMusicPrototype? _musicProto;

    // Need to keep track of the last biome we were in to re-play its music when we're out of combat mode
    private SpaceBiomePrototype? _lastBiome;

    // Every <THIS> amount of time, attempt to play a new music track. This ticks down on rejoining as well.
    private TimeSpan _timeUntilNextAmbientTrack = TimeSpan.FromMinutes(5);

    // List of available ambient music tracks to sift through.
    private List<AmbientMusicPrototype>? _musicTracks;

    // Time in seconds for ambient music tracks to fade in. Set to 0 to play immediately.
    private float _ambientMusicFadeInTime = 10f;

    // Time in seconds for combat music tracks to fade in. Set to 0 to play immediately.
    private float _combatMusicFadeInTime = 2f;

    // Time that combat mode needs to be on to start playing music. Set to 0 to play immediately.
    private TimeSpan _combatStartUpTime = TimeSpan.FromSeconds(2.0);

    // Time that combat mode needs to be off to stop combat mode. Set to 0 to turn off as soon as combat mode is off.
    private TimeSpan _combatWindDownTime = TimeSpan.FromSeconds(20.0);

    // Combat mode state before checking to switch combat music off/on.
    // 1. We toggle combat mode. We fire SwitchCombatMusic in (timer) seconds.
    // 2. We save the state from step 1 in _lastCombatState
    // 3. When SwitchCombatMusic fires, we check if the current combat state is different than _lastCombatState. If it is, then we change music. If not, we keep it.
    bool _lastCombatState = false;

    private ISawmill _sawmill = default!;

    private void InitializeAmbientMusic()
    {
        SubscribeNetworkEvent<SpaceBiomeSwapMessage>(OnBiomeChange);
        SubscribeLocalEvent<ToggleCombatActionEvent>(OnCombatModeToggle);

        Subs.CVar(_configManager, CCVars.AmbientMusicVolume, AmbienceCVarChanged, true);
        _sawmill = IoCManager.Resolve<ILogManager>().GetSawmill("audio.ambience");

        // Setup tracks to pull from. Runs once.
        _musicTracks = GetTracks();

        Timer.Spawn(_timeUntilNextAmbientTrack, () => ReplayAmbientMusic());

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnProtoReload);
        _state.OnStateChanged += OnStateChange;
        // On round end summary OR lobby cut audio.
        SubscribeNetworkEvent<RoundEndMessageEvent>(OnRoundEndMessage);
    }


    /// <summary>
    /// This function runs on a timer to check if music is playing or not, and play it again.
    /// </summary>
    private void ReplayAmbientMusic()
    {
        bool? isDone = null;

        if (TryComp(_ambientMusicStream, out AudioComponent? audioComp))
        {
            isDone = !audioComp.Playing;
        }

        if (isDone == true) //if it's not done, this just does nothing
        {
            if (_musicProto == null) //if we don't find any, we play the default track.
            {
                _musicProto = _proto.Index<AmbientMusicPrototype>("default");
                _lastBiome = _proto.Index<SpaceBiomePrototype>("default");
            }

            SoundCollectionPrototype soundcol = _proto.Index<SoundCollectionPrototype>(_musicProto.ID);

            string path = _random.Pick(soundcol.PickFiles).ToString(); // THIS WILL PICK A RANDOM SOUND. WE MAY WANT TO SPECIFY ONE INSTEAD!!

            PlayMusicTrack(path, _musicProto.Sound.Params.Volume, _ambientMusicFadeInTime);

            Timer.Spawn(_timeUntilNextAmbientTrack, () => ReplayAmbientMusic());
        }

    }

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

        PlayMusicTrack(path, _musicProto.Sound.Params.Volume, _ambientMusicFadeInTime);
    }


    private void OnCombatModeToggle(ToggleCombatActionEvent ev)
    {
        if (!_timing.IsFirstTimePredicted == true) //needed, because combat mode is predicted, and triggers 7 times otherwise.
            return;

        bool currentCombatState = _combatModeSystem.IsInCombatMode();


        if (currentCombatState)
            Timer.Spawn(_combatStartUpTime, SwitchCombatMusic);
        else
            Timer.Spawn(_combatWindDownTime, SwitchCombatMusic);;

    }
    private void SwitchCombatMusic()
    {
        bool currentCombatState = _combatModeSystem.IsInCombatMode();

        if (_lastCombatState == currentCombatState)
            return;

        _lastCombatState = currentCombatState;

        FadeOut(_ambientMusicStream);

        if (currentCombatState) //true = we toggled combat ON. 
        {
            string combatFactionSuffix = ""; //this is added to "combatmode" to create "combatmodeNCWL", "combatmodeDSM", etc, to fetch combat tracks.

            if (_prefsManager.Preferences != null) //this literally cannot be null unless you're in lobby or something
            {
                var profile = (HumanoidCharacterProfile) _prefsManager.Preferences.SelectedCharacter;

                combatFactionSuffix = profile.Faction; //becomes NCWL, DSM, etc.

                //_sawmill.Debug($"FACTION: {faction}");
            }

            //if we find a ambient music prototype for our faction, then pick that one!
            if (_proto.TryIndex<AmbientMusicPrototype>("combatmode" + combatFactionSuffix, out var factionCombatMusicPrototype))
            {
                _musicProto = factionCombatMusicPrototype;
                SoundCollectionPrototype soundcol = _proto.Index<SoundCollectionPrototype>(_musicProto.ID);

                string path = _random.Pick(soundcol.PickFiles).ToString(); // THIS WILL PICK A RANDOM SOUND. WE MAY WANT TO SPECIFY ONE INSTEAD!!

                PlayMusicTrack(path, _musicProto.Sound.Params.Volume, _combatMusicFadeInTime);
            }
            else //if the faction combat music prototype does not exist, instead fall back to the default.
            {
                _musicProto = _proto.Index<AmbientMusicPrototype>("combatmodedefault");
                SoundCollectionPrototype soundcol = _proto.Index<SoundCollectionPrototype>(_musicProto.ID); //THIS IS WHAT ERRORS!

                string path = _random.Pick(soundcol.PickFiles).ToString(); // THIS WILL PICK A RANDOM SOUND. WE MAY WANT TO SPECIFY ONE INSTEAD!!

                PlayMusicTrack(path, _musicProto.Sound.Params.Volume, _combatMusicFadeInTime);
            }
        }
        else                    //false = we toggled combat OFF
        {
            if (_lastBiome == null) //this should never happen still
            {
                if (_player.LocalSession != null) //THIS LITERALLY CANNOT BE NULL!! BUT IT COMPLAINS IF I DONT PUT THIS HERE!!!
                {
                    _entMan.TryGetComponent<SpaceBiomeTrackerComponent>(_player.LocalSession.AttachedEntity, out var comp);
                    if (comp != null)
                    {
                        if (comp.Biome != null)
                            _lastBiome = _proto.Index<SpaceBiomePrototype>(comp.Biome);
                    }
                }
            }

            if (_lastBiome == null)
                return;

            _musicProto = _proto.Index<AmbientMusicPrototype>(_lastBiome.ID); //THIS CAN FUCK UP! BECAUSE THE ID MIGHT NOT HAVE MUSIC AND BE A FALLBACK!

            SoundCollectionPrototype soundcol = _proto.Index<SoundCollectionPrototype>(_musicProto.ID); //THIS IS WHAT ERRORS!

            string path = _random.Pick(soundcol.PickFiles).ToString(); // THIS WILL PICK A RANDOM SOUND. WE MAY WANT TO SPECIFY ONE INSTEAD!!

            PlayMusicTrack(path, _musicProto.Sound.Params.Volume, _ambientMusicFadeInTime);
        }
    }

    /// <summary>
    /// This is a helper function that actually plays the music tracks.
    /// </summary>
    /// <param name="path"> Path to music to play.</param>
    /// <param name="volume"> Volume modifier (put 0 to keep original volume).</param>
    /// <param name="fadein"> Seconds for the music to fade in. Put 0 for no fadein. </param>
    private void PlayMusicTrack(string path, float volume, float fadein)
    {
        _sawmill.Debug($"NOW PLAYING: {path}");

        var strim = _audio.PlayGlobal(
            path,
            Filter.Local(),
            false,
            AudioParams.Default.WithVolume(volume + _volumeSlider))!;

        _ambientMusicStream = strim.Value.Entity; //this plays it immediately, but fadein function later makes it actually fade in.

        if (fadein != 0)
            FadeIn(_ambientMusicStream, strim.Value.Component, fadein);
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
            _sawmill.Debug($"NO MUSIC FOUND, SOMETHING IS WRONG!");
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
    /// This function handles the change from lobby to gameplay, disabling music when you're not in gameplay state.
    ///</summary>
    private void OnStateChange(StateChangedEventArgs obj)
    {
        if (obj.NewState is not GameplayState)
            DisableAmbientMusic();
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
