using Content.Server.Chat.Systems;
using Content.Server.Mapping;
using Content.Server.Radio.Components;
using Content.Server.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Radio;
using Content.Shared.Sound.Components;
using Robust.Server.GameStates;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Systems;
using Content.Shared._Crescent.Broadcaster;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using static Content.Shared.Access.Components.IdCardConsoleComponent;
using Content.Shared.Access.Components;
using Robust.Server.GameObjects;
using Robust.Server.Player;

namespace Content.Server._Crescent.Broadcaster;

/// <summary>
/// This is used for...
/// </summary>W
public sealed partial class BroadcasterSystem : SharedBroadcasterSystem

{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly ChatSystem _chatting = default!;



    private List<BroadcastWrapper> broadcastableMessages = new();

    private Dictionary<string, int> currentlyPlayingOn = new();
    private Dictionary<string, float> playtimesLeft = new();


    private struct BroadcastWrapper
    {
        public SoundPathSpecifier sound;
        public float duration;
        public string name;
        public string outpost;
        public string text;

        public BroadcastWrapper(SoundPathSpecifier playing, float dur, string name, string outpost, string text)
        {
            sound = playing;
            duration = dur;
            this.name = name;
            this.outpost = outpost;
            this.text = text;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        var messages = _prototypeManager.EnumeratePrototypes<BroadcastableMessagePrototype>();
        foreach (var message in messages)
        {
            if (message.Outpost is null)
                continue;
            broadcastableMessages.Add(new BroadcastWrapper(message.announceSound, (float)_audioSystem.GetAudioLength(message.announceSound.Path.ToRootedPath().CanonPath).TotalSeconds, message.Name, message.Outpost, message.Text));
            currentlyPlayingOn.TryAdd(message.Outpost, -1);
        }
        SubscribeLocalEvent<BroadcastingConsoleComponent, ComponentStartup>(RequestAvailableBroadcasts);
        SubscribeLocalEvent<BroadcastingConsoleComponent, BroadcasterBroadcastMessage>(PlayBroadcast);
    }

    private Dictionary<int, string> buildBroadcastListForState(string outpost)
    {

        var dict = new Dictionary<int, string>();

        for (int i = 0; i < broadcastableMessages.Count; i++)
        {
            var message = broadcastableMessages[i];
            if (message.outpost != outpost)
                continue;
            dict.Add(i, message.name);
        }

        return dict;

    }

    public void RequestAvailableBroadcasts(EntityUid uid, BroadcastingConsoleComponent comp, ref ComponentStartup args)
    {
        if (comp.Outpost is null)
            return;
        if (!currentlyPlayingOn.ContainsKey(comp.Outpost))
            currentlyPlayingOn.Add(comp.Outpost, -1);
        comp.currentlyPlaying = currentlyPlayingOn[comp.Outpost];
        comp.AvailableAnnouncements = buildBroadcastListForState(comp.Outpost);
        EntityManager.Dirty(new Entity<BroadcastingConsoleComponent>(uid, comp));
        var newState = new BroadcasterConsoleState(buildBroadcastListForState(comp.Outpost), currentlyPlayingOn[comp.Outpost]);
        _userInterface.SetUiState(uid, BroadcasterUIKey.Key, newState);
    }

    public void RequestAvailableBroadcasts(EntityUid uid, BroadcastingConsoleComponent comp)
    {
        if (comp.Outpost is null)
            return;
        comp.currentlyPlaying = currentlyPlayingOn[comp.Outpost];
        comp.AvailableAnnouncements = buildBroadcastListForState(comp.Outpost);
        EntityManager.Dirty(new Entity<BroadcastingConsoleComponent>(uid, comp));
        var newState = new BroadcasterConsoleState(buildBroadcastListForState(comp.Outpost), currentlyPlayingOn[comp.Outpost]);
        _userInterface.SetUiState(uid, BroadcasterUIKey.Key, newState);
    }

    public void UpdateAllConsoles(string targetOutpost)
    {
        var comps = EntityManager.GetAllComponents(typeof(BroadcastingConsoleComponent));
        foreach (var comp in comps)
        {
            var cast = (BroadcastingConsoleComponent) comp.Component;
            if (cast.Outpost is null)
                continue;
            if (cast.Outpost != targetOutpost)
                continue;
            RequestAvailableBroadcasts(comp.Uid, cast);
        }
    }


    public void UpdateAllConsoles()
    {
        var comps = EntityManager.GetAllComponents(typeof(BroadcastingConsoleComponent));
        foreach (var comp in comps)
        {
            RequestAvailableBroadcasts(comp.Uid, (BroadcastingConsoleComponent) comp.Component);
        }
    }

    public void PlayBroadcast(EntityUid uid, BroadcastingConsoleComponent comp, ref BroadcasterBroadcastMessage args)
    {
        if (args.indexForBroadcast > broadcastableMessages.Count)
            return;
        var message = broadcastableMessages[args.indexForBroadcast];
        if (message.outpost != comp.Outpost)
            return;
        if (currentlyPlayingOn[message.outpost] != -1)
            return;
        currentlyPlayingOn[message.outpost] = args.indexForBroadcast;
        if (comp.AvailableAnnouncements is null)
            return;
        var comps = EntityManager.GetAllComponents(typeof(BroadcasterComponent));
        playtimesLeft.Add(comp.Outpost, broadcastableMessages[args.indexForBroadcast].duration);
        UpdateAllConsoles(comp.Outpost);
        //HashSet<Entity<EyeComponent>> alreadyMessaged = new();
        foreach (var broadcaster in comps)
        {
            var broadcastingComp = (BroadcasterComponent)broadcaster.Component;
            HashSet<Entity<EyeComponent>> targets = new();
            _lookup.GetEntitiesInRange<EyeComponent>(_transform.GetMapCoordinates(broadcaster.Uid,
                Transform(broadcaster.Uid)), broadcastingComp.Range, targets, LookupFlags.All);
            foreach(var player in targets)
            {
                if (!_playerManager.TryGetSessionByEntity(player.Owner, out var _))
                    continue;
                //if(alreadyMessaged.Contains(player))
                //     _chatting.TrySendInGameICMessage(broadcaster.Uid, broadcastableMessages[args.indexForBroadcast].text, InGameICChatType.Speak, ChatTransmitRange.Normal);
                //    continue;
                //alreadyMessaged.Add(player);

                _chatting.TrySendInGameICMessage(broadcaster.Uid, broadcastableMessages[args.indexForBroadcast].text, InGameICChatType.Speak, ChatTransmitRange.Normal);
                _audioSystem.PlayEntity(broadcastableMessages[args.indexForBroadcast].sound, player.Owner, broadcaster.Uid);
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        foreach (var (key, timespan) in playtimesLeft)
        {
            playtimesLeft[key] -= frameTime;
            if (playtimesLeft[key] <= 0)
            {
                currentlyPlayingOn[key] = -1;
                playtimesLeft.Remove(key);
                UpdateAllConsoles(key);
            }
        }
    }
}
