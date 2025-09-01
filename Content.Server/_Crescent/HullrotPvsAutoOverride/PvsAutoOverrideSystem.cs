using System.Collections.Frozen;
using System.Linq;
using Content.Shared._Crescent.PvsAutoOverride;
using Content.Shared.Players;
using Robust.Server.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Player;


namespace Content.Server._Crescent.HullrotPvsAutoOverride;


/// <summary>
/// This handles...
/// </summary>
public sealed class PvsAutoOverrideSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ISharedPlayerManager _players = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    [Dependency] private readonly PvsOverrideSystem _override = default!;
    private float accumulator = 0f;

    // first is player EntityUid , second is ownerUid, third is component
    private FrozenDictionary<IComponent, Func<EntityUid, EntityUid, IComponent, bool>> pvsConditions =
        new Dictionary<IComponent, Func<EntityUid, EntityUid, IComponent, bool>>().ToFrozenDictionary();

    private FrozenDictionary<IComponent, EntityQuery<IComponent>> compToQuery =
        new Dictionary<IComponent, EntityQuery<IComponent>>().ToFrozenDictionary();


    public void AddPvsCondition(IComponent component, Func<EntityUid, EntityUid, IComponent, bool> condition)
    {
        Dictionary<IComponent, Func<EntityUid, EntityUid, IComponent, bool>> reconstructing =
            pvsConditions.ToDictionary();
        reconstructing.Add(component, condition);
        Dictionary<IComponent, EntityQuery<IComponent>> queries = compToQuery.ToDictionary();
        queries.Add(component, EntityManager.GetEntityQuery(component.GetType()));
        pvsConditions = reconstructing.ToFrozenDictionary();
        compToQuery = queries.ToFrozenDictionary();
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        accumulator += frameTime;
        if (accumulator < 1f)
            return;
        accumulator = 0f;
        var playerData = _players.GetAllPlayerData();
        var enumerator = EntityManager.EntityQueryEnumerator<PvsAutoOverrideComponent>();
        HashSet<(EntityUid, ICommonSession)> validPlayers = new();
        foreach (var player in playerData)
        {
            var session = _players.GetSessionById(player.UserId);
            if(session.AttachedEntity is not null)
                validPlayers.Add((session.AttachedEntity.Value, session));
        }

        while (enumerator.MoveNext(out var uid, out var comp))
        {
            foreach (var (player, session) in validPlayers)
            {
                if ((_transform.GetWorldPosition(uid) - _transform.GetWorldPosition(player)).Length() > comp.PvsSendRange)
                {
                    goto RemoveSession;
                }

                foreach (var (queryComp, entQuery) in compToQuery)
                {
                    if (!entQuery.HasComp(uid))
                        continue;
                    // just need one to be fulfilled
                    if (pvsConditions[queryComp].Invoke(player, uid, comp))
                        break;
                    goto RemoveSession;
                }

                if (comp.SendingSessions.Contains(session))
                    continue;
                comp.SendingSessions.Add(session);
                _override.AddSessionOverride(uid, session);
                continue;

                RemoveSession:
                if (comp.SendingSessions.Remove(session))
                {
                    _override.RemoveSessionOverride(uid, session);
                }
            }
        }
    }
}

