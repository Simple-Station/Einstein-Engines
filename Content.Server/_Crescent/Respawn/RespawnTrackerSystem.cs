using System.Diagnostics.CodeAnalysis;
using Content.Server._Crescent.Respawn.Components;
using Content.Shared.Mobs;
using Content.Shared.Crescent.Ghost;
using Content.Shared.GameTicking;
using Content.Shared.Mind.Components;
using Content.Shared.Mind;
using Content.Server.Mind;
using Content.Shared._Crescent.CCvars;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._Crescent.Respawn;

public sealed class RespawnTrackerSystem : EntitySystem
{
    private const string RespawnEntityPrototype = "GhostRespawnTicker";

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    private EntityUid? _respawnEntity;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
        SubscribeLocalEvent<MindContainerComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<PlayerSessionEntityDeletedEvent>(OnEntityDeleted);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);

        SubscribeNetworkEvent<RespawnTimeRequestEvent>(OnRespawnTimeRequest);
    }

    private void OnRoundStarted(RoundStartedEvent ev)
    {
        _respawnEntity = Spawn(RespawnEntityPrototype, MapCoordinates.Nullspace);
    }

    private void OnMobStateChanged(EntityUid uid, MindContainerComponent component, MobStateChangedEvent args)
    {
        // User ID is stored in the mind, even if they are disconnected or w/e
        if (!TryComp<MindComponent>(component.Mind, out var mind) || mind.UserId is null)
            return;

        var guid = mind.UserId.Value.UserId;

        // erm, you're dead
        if (args.NewMobState == MobState.Dead)
            AddEntry(guid);

        // erm, you're not dead anymore somehow
        if (args.OldMobState == MobState.Dead && args.NewMobState != MobState.Dead)
            RemoveEntry(guid);
    }

    private void OnEntityDeleted(ref PlayerSessionEntityDeletedEvent args)
    {
        if (!TryGetTrackers(out var trackers))
            return;

        // don't bully this guy if it was his CORPSE that got deleted
        if (trackers.ContainsKey(args.Guid))
            return;

        // otherwise we probably got instagibbed or eaten by singulo or something
        AddEntry(args.Guid);
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _respawnEntity = null;
    }

    private void OnRespawnTimeRequest(RespawnTimeRequestEvent ev, EntitySessionEventArgs args)
    {
        if (!TryGetTrackers(out var trackers))
            return;

        var guid = args.SenderSession.UserId.UserId;
        var respawnTime = _timing.CurTime;

        if (trackers.TryGetValue(guid, out var value))
            respawnTime = value;

        var response = new RespawnTimeResponseEvent(respawnTime);
        RaiseNetworkEvent(response, args.SenderSession.Channel);
    }

    public bool CheckRespawn(Guid guid)
    {
        if (!TryGetTrackers(out var trackers))
            return true;

        if (!trackers.TryGetValue(guid, out var respawnTime))
            return true;

        if (_timing.CurTime < respawnTime)
            return false;

        RemoveEntry(guid);
        return true;
    }

    private void AddEntry(Guid guid)
    {
        if (!TryGetTrackers(out var trackers))
            return;

        // delete your old entry if you have one
        RemoveEntry(guid);

        // add new entry
        trackers.Add(guid, _timing.CurTime + TimeSpan.FromSeconds(_cfg.GetCVar(CrescentCVars.RespawnTime)));
    }

    private bool TryGetTrackers([NotNullWhen(true)] out Dictionary<Guid, TimeSpan>? trackers)
    {
        trackers = null;

        if (_respawnEntity is null || !TryComp<RespawnTickerComponent>(_respawnEntity, out var comp))
            return false;

        trackers = comp.RespawnTrackers;
        return true;
    }

    /// <summary>
    /// Removes an entry from the respawn tracker
    /// </summary>
    /// <param name="guid">Player guid</param>
    /// <returns>Whether an entry was removed</returns>
    private bool RemoveEntry(Guid guid)
    {
        return TryGetTrackers(out var trackers) && trackers.Remove(guid);
    }
}
