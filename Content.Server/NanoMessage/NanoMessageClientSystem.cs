using System.Linq;
using Content.Shared.NanoMessage.Data;
using Robust.Shared.Random;
using Robust.Shared.Timing;


namespace Content.Server.NanoMessage;

public sealed partial class NanoMessageClientSystem : EntitySystem
{
    [Dependency] private readonly NanoMessageServerSystem _servers = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<NanoMessageClientComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<NanoMessageClientComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextReconnectAttempt)
                continue;
            comp.NextReconnectAttempt = _timing.CurTime + comp.ReconnectInterval;

            if (!IsServerValid((uid, comp), comp.ConnectedServer))
                TryReconnect((uid, comp));
        }
        query.Dispose();
    }

    private void OnMapInit(Entity<NanoMessageClientComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.Id > 0)
            return;

        // 32 bits of entropy should be enough to never have collisions
        ent.Comp.Id = (ulong) _random.Next();
    }

    public bool IsServerValid(Entity<NanoMessageClientComponent> client, EntityUid server)
    {
        if (server is not { Valid: true } || Transform(client).GridUid != Transform(server).GridUid)
            return false;

        return _servers.IsServerActive(server);
    }

    public bool TryReconnect(Entity<NanoMessageClientComponent> ent)
    {
        var result = FindClosestServer(ent);
        if (result is not { Valid: true })
        {
            if (ent.Comp.ConnectedServer is { Valid: true })
                _servers.Disconnect(ent.Comp.ConnectedServer, ent!);
            return false;
        }

        return _servers.TryConnect(result, ent!);
    }

    public bool TrySendMessage(Entity<NanoMessageClientComponent?> client, ulong conversationId, string message)
    {
        if (!Resolve(client, ref client.Comp)
            || client.Comp.Id <= 0
            || client.Comp.ConnectedServer is not { Valid: true } server)
            return false;

        var msg = new NanoMessageMessage
        {
            Content = message,
            Sender = client.Comp.Id,
            Timestamp = _timing.CurTime
        };

        return _servers.TryDispatchMessage(server, conversationId, msg);
    }

    private EntityUid FindClosestServer(Entity<NanoMessageClientComponent> ent)
    {
        var xform = Transform(ent);
        if (xform.GridUid is not { Valid: true } clientGrid)
            return EntityUid.Invalid;

        // If there are multiple on this grid, the first one with the highest priority will be chosen
        var candidates = EntityQuery<NanoMessageServerComponent, TransformComponent>()
            .Where(s => s.Item2.GridUid == clientGrid)
            .Where(s => _servers.IsServerActive((s.Item1.Owner, s.Item1)))
            .ToList();

        if (candidates.Count == 0)
            return EntityUid.Invalid;

        // In case multiple servers have the same highest priority, choose the one with lowest uid to avoid issues.
        var sample = candidates.MaxBy(s => s.Item1.Priority);
        return candidates.Where(s => s.Item1.Priority == sample.Item1.Priority)
            .Select(s => s.Item1.Owner)
            .Min();
    }
}
