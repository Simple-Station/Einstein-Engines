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
using Content.Server.Backmen.Blob.Components;
using Content.Shared.Backmen.Blob.Components;
using Content.Shared.Destructible;
using Robust.Server.GameObjects;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
>>>>>>> a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Blob;

public sealed class BlobNodeSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

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

        SubscribeLocalEvent<BlobNodeComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, BlobNodeComponent component, ComponentStartup args)
    {

||||||| parent of a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
        _tileQuery = GetEntityQuery<BlobTileComponent>();
=======
        _tileQuery = GetEntityQuery<BlobTileComponent>();

>>>>>>> a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
    }

    private const double PulseJobTime = 0.005;
    private readonly JobQueue _pulseJobQueue = new(PulseJobTime);

    public sealed class BlobPulse : Job<object>
    {
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

        protected override async Task<object?> Process()
        {
            _system.Pulse(_ent);
            return null;
        }
    }

    private void Pulse(Entity<BlobNodeComponent> ent)
    {
        var xform = Transform(ent);

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
        var innerTiles = grid.GetLocalTilesIntersecting(
            new Box2(localPos + new Vector2(-radius, -radius), localPos + new Vector2(radius, radius)), false).ToArray();
||||||| parent of a2da11302d (cleanup blob (#598)):Content.Server/Backmen/Blob/BlobNodeSystem.cs
        var innerTiles = _map.GetLocalTilesIntersecting(xform.GridUid.Value, grid,
            new Box2(localPos + new Vector2(-radius, -radius), localPos + new Vector2(radius, radius)), false).ToArray();
=======
        var innerTiles = _map.GetLocalTilesIntersecting(xform.GridUid.Value, grid,
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
                if (!_tileQuery.HasComp(tile))
                    continue;

                var ev = new BlobTileGetPulseEvent
                {
                    Explain = explain
                };
                RaiseLocalEvent(tile, ev);
                explain = false;
            }
        }

        foreach (var lookupUid in _lookup.GetEntitiesInRange<BlobMobComponent>(xform.Coordinates, radius))
        {
            var ev = new BlobMobGetPulseEvent();
            RaiseLocalEvent(lookupUid, ev);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _pulseJobQueue.Process();

        var blobFactoryQuery = EntityQueryEnumerator<BlobNodeComponent>();
        while (blobFactoryQuery.MoveNext(out var ent, out var comp))
        {
            if (_gameTiming.CurTime < comp.NextPulse)
                return;

            if (TryComp<BlobTileComponent>(ent, out var blobTileComponent) && blobTileComponent.Core != null)
            {
                _pulseJobQueue.EnqueueJob(new BlobPulse(this,(ent, comp),PulseJobTime));
            }

            comp.NextPulse = _gameTiming.CurTime + TimeSpan.FromSeconds(comp.PulseFrequency);
        }
    }
}
