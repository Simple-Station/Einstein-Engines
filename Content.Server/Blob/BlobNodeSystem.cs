using System.Linq;
using System.Numerics;
<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs
||||||| parent of a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
using Content.Server.Backmen.Blob.Components;
using Content.Shared.Backmen.Blob.Components;
using Robust.Server.GameObjects;
=======
using System.Threading;
using System.Threading.Tasks;
using Content.Shared.Backmen.Blob.Components;
using Content.Shared.Destructible;
using Content.Shared.Mobs.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs
>>>>>>> a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
using Robust.Shared.Map;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
=======
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
using Robust.Shared.Random;

namespace Content.Server.Blob;

public sealed class BlobNodeSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly BlobCoreSystem _blobCoreSystem = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;

<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs
||||||| parent of a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
    private EntityQuery<BlobTileComponent> _tileQuery;
=======
    private EntityQuery<BlobTileComponent> _tileQuery;

>>>>>>> a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
    public override void Initialize()
    {
        base.Initialize();
<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs
<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs

        SubscribeLocalEvent<BlobNodeComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, BlobNodeComponent component, ComponentStartup args)
    {

||||||| parent of a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
        _tileQuery = GetEntityQuery<BlobTileComponent>();
=======
        _tileQuery = GetEntityQuery<BlobTileComponent>();
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
        _tileQuery = GetEntityQuery<BlobTileComponent>();
=======
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs

<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs
>>>>>>> a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
=======
        SubscribeLocalEvent<BlobNodeComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<BlobNodeComponent, EntityTerminatingEvent>(OnTerminating);
        SubscribeLocalEvent<BlobNodeComponent, BlobNodePulseEvent>(OnNodePulse);

        _tileQuery = GetEntityQuery<BlobTileComponent>();
    }

    private void OnNodePulse(Entity<BlobNodeComponent> ent, ref BlobNodePulseEvent args)
    {
        var xform = Transform(ent);

        var evSpecial = new BlobSpecialGetPulseEvent();
        foreach (var special in GetSpecialBlobsTiles(ent))
        {
            RaiseLocalEvent(special, evSpecial);
        }

        foreach (var lookupUid in _lookup.GetEntitiesInRange<BlobMobComponent>(xform.Coordinates, ent.Comp.PulseRadius))
        {
            if(_mob.IsDead(lookupUid))
                continue;
            var evMob = new BlobMobGetPulseEvent
            {
                BlobEntity = GetNetEntity(lookupUid),
            };
            RaiseLocalEvent(lookupUid, evMob);
            RaiseNetworkEvent(evMob, Filter.Pvs(lookupUid));
        }
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
    }

    private const double PulseJobTime = 0.005;
    private readonly JobQueue _pulseJobQueue = new(PulseJobTime);

    public sealed class BlobPulse(
        BlobNodeSystem system,
        Entity<BlobNodeComponent> ent,
        double maxTime,
        CancellationToken cancellation = default)
        : Job<object>(maxTime, cancellation)
    {
<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs
        private readonly BlobNodeSystem _system;
        private readonly Entity<BlobNodeComponent> _ent;

        public BlobPulse(BlobNodeSystem system, Entity<BlobNodeComponent> ent, double maxTime,
            CancellationToken cancellation = default) : base(maxTime, cancellation)
        {
            _system = system;
            _ent = ent;
        }

        public BlobPulse(BlobNodeSystem system, Entity<BlobNodeComponent> ent, double maxTime, IStopwatch stopwatch,
            CancellationToken cancellation = default) : base(maxTime, stopwatch, cancellation)
        {
            _system = system;
            _ent = ent;
        }

||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
        private readonly BlobNodeSystem _system;
        private readonly Entity<BlobNodeComponent> _ent;

        public BlobPulse(BlobNodeSystem system,
            Entity<BlobNodeComponent> ent,
            double maxTime,
            CancellationToken cancellation = default) : base(maxTime, cancellation)
        {
            _system = system;
            _ent = ent;
        }

        public BlobPulse(BlobNodeSystem system,
            Entity<BlobNodeComponent> ent,
            double maxTime,
            IStopwatch stopwatch,
            CancellationToken cancellation = default) : base(maxTime, stopwatch, cancellation)
        {
            _system = system;
            _ent = ent;
        }

=======
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
        protected override async Task<object?> Process()
        {
            system.Pulse(ent);
            return null;
        }
    }

    private void OnTerminating(EntityUid uid, BlobNodeComponent component, ref EntityTerminatingEvent args)
    {
        OnDestruction(uid, component, new DestructionEventArgs());
    }

    private IEnumerable<Entity<BlobTileComponent>> GetSpecialBlobsTiles(BlobNodeComponent component)
    {
        if (!TerminatingOrDeleted(component.BlobFactory) && _tileQuery.TryComp(component.BlobFactory, out var tileFactoryComponent))
        {
            yield return (component.BlobFactory.Value, tileFactoryComponent);
        }
        if (!TerminatingOrDeleted(component.BlobResource) && _tileQuery.TryComp(component.BlobResource, out var tileResourceComponent))
        {
            yield return (component.BlobResource.Value, tileResourceComponent);
        }
    }

    private void OnDestruction(EntityUid uid, BlobNodeComponent component, DestructionEventArgs args)
    {
        if (!TryComp<BlobTileComponent>(uid, out var tileComp) ||
            tileComp.BlobTileType != BlobTileType.Node ||
            tileComp.Core == null)
            return;

        foreach (var tile in GetSpecialBlobsTiles(component))
        {
            tile.Comp.ReturnCost = false;
            _blobCoreSystem.RemoveTileWithReturnCost(tile, tile.Comp.Core!.Value);
        }
    }

    private void Pulse(Entity<BlobNodeComponent> ent)
    {
<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs
        var xform = Transform(ent);
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
        if(TerminatingOrDeleted(ent) || !EntityManager.TransformQuery.TryComp(ent, out var xform))
            return;
=======
        if (TerminatingOrDeleted(ent) || !EntityManager.TransformQuery.TryComp(ent, out var xform))
            return;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs

        var radius = ent.Comp.PulseRadius;

        var localPos = xform.Coordinates.Position;

        if (!_map.TryGetGrid(xform.GridUid, out var grid))
        {
            return;
        }

<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs
        if (!TryComp<BlobTileComponent>(uid, out var blobTileComponent) || blobTileComponent.Core == null)
||||||| parent of a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
        if (!_tileQuery.TryGetComponent(uid, out var blobTileComponent) || blobTileComponent.Core == null)
=======
        if (!_tileQuery.TryGetComponent(ent, out var blobTileComponent) || blobTileComponent.Core == null)
>>>>>>> a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
            return;

<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs
<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs
        var innerTiles = grid.GetLocalTilesIntersecting(
            new Box2(localPos + new Vector2(-radius, -radius), localPos + new Vector2(radius, radius)), false).ToArray();
||||||| parent of a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
        var innerTiles = _map.GetLocalTilesIntersecting(xform.GridUid.Value, grid,
            new Box2(localPos + new Vector2(-radius, -radius), localPos + new Vector2(radius, radius)), false).ToArray();
=======
        var innerTiles = _map.GetLocalTilesIntersecting(xform.GridUid.Value, grid,
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
        var innerTiles = _map.GetLocalTilesIntersecting(xform.GridUid.Value, grid,
=======
        var innerTiles = _map.GetLocalTilesIntersecting(xform.GridUid.Value,
                grid,
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
            new Box2(localPos + new Vector2(-radius, -radius), localPos + new Vector2(radius, radius)),
            false).ToArray();
>>>>>>> a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs

        _random.Shuffle(innerTiles);

        var explain = true;
        foreach (var tileRef in innerTiles)
        {
<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs
            foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
||||||| parent of a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
            foreach (var ent in _map.GetAnchoredEntities(xform.GridUid.Value, grid, tileRef.GridIndices))
=======
            foreach (var tile in _map.GetAnchoredEntities(xform.GridUid.Value, grid, tileRef.GridIndices))
>>>>>>> a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
            {
                if (!_tileQuery.HasComponent(tile))
                    continue;

                var ev = new BlobTileGetPulseEvent
                {
                    Handled = explain
                };
                RaiseLocalEvent(tile, ev);
                explain = false; // WTF?
            }
        }

        RaiseLocalEvent(ent, new BlobNodePulseEvent());
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _pulseJobQueue.Process();

        var blobNodeQuery = EntityQueryEnumerator<BlobNodeComponent, BlobTileComponent>();
        while (blobNodeQuery.MoveNext(out var ent, out var comp, out var blobTileComponent))
        {
<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs
            if (_gameTiming.CurTime < comp.NextPulse)
                return;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
            if (_gameTiming.CurTime < comp.NextPulse)
                continue;
=======
            comp.NextPulse += frameTime;
            if (comp.PulseFrequency > comp.NextPulse)
                continue;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs

<<<<<<< HEAD:Content.Server/Blob/BlobNodeSystem.cs
            if (TryComp<BlobTileComponent>(ent, out var blobTileComponent) && blobTileComponent.Core != null)
            {
                _pulseJobQueue.EnqueueJob(new BlobPulse(this,(ent, comp),PulseJobTime));
            }
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
            if (_tileQuery.TryGetComponent(ent, out var blobTileComponent) && blobTileComponent.Core != null)
            {
                _pulseJobQueue.EnqueueJob(new BlobPulse(this,(ent, comp),PulseJobTime));
            }
=======
            comp.NextPulse -= comp.PulseFrequency;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobNodeSystem.cs

            if (blobTileComponent.Core == null)
            {
                QueueDel(ent);
                continue;
            }
            _pulseJobQueue.EnqueueJob(new BlobPulse(this,(ent, comp), PulseJobTime));
        }
    }
}
