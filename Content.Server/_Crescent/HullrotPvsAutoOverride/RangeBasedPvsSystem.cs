using System.Collections.Frozen;
using System.Linq;
using System.Runtime.Remoting;
using Content.Shared._Crescent.PvsAutoOverride;
using Content.Shared.Players;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Player;


namespace Content.Server._Crescent.HullrotPvsAutoOverride;


/// <summary>
/// This handles...
/// </summary>
public sealed class RangeBasedPvsSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ISharedPlayerManager _players = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PvsOverrideSystem _override = default!;
    private float accumulator = 0f;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RangeBasedPvsComponent, ComponentRemove>(OnFree);
    }

    public override void Shutdown()
    {
        var enumerator = EntityManager.EntityQueryEnumerator<RangeBasedPvsComponent>();
        while (enumerator.MoveNext(out var uid, out var comp))
        {
            foreach (var session in comp.SendingSessions)
            {
                _override.RemoveSessionOverride(uid, session);
            }
            comp.SendingSessions.Clear();
        }


        base.Shutdown();

    }

    public void OnFree(Entity<RangeBasedPvsComponent> obj, ref ComponentRemove args)
    {
        foreach (var session in obj.Comp.SendingSessions)
        {
            _override.RemoveSessionOverride(obj.Owner, session);
        }
        obj.Comp.SendingSessions.Clear();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var playerData = _players.GetAllPlayerData();
        var enumerator = EntityManager.EntityQueryEnumerator<RangeBasedPvsComponent>();
        HashSet<(EntityUid, ICommonSession)> validPlayers = new();
        foreach (var player in playerData)
        {
            if (!_players.TryGetSessionById(player.UserId, out var session))
                continue;
            if(session.AttachedEntity is not null)
                validPlayers.Add((session.AttachedEntity.Value, session));
        }

        while (enumerator.MoveNext(out var uid, out var comp))
        {
            foreach (var (player, session) in validPlayers)
            {
                if ((_transform.GetWorldPosition(uid) - _transform.GetWorldPosition(player)).Length() > comp.PvsSendRange)
                {
                    if (comp.SendingSessions.Remove(session))
                    {
                        _override.RemoveSessionOverride(uid, session);
                    }

                    continue;
                }

                if (comp.SendingSessions.Contains(session))
                    continue;
                comp.SendingSessions.Add(session);
                _override.AddSessionOverride(uid, session);
            }
        }
    }
}

