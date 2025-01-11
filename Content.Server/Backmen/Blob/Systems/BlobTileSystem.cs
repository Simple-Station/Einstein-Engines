using System.Linq;
using System.Numerics;
using Content.Server.Construction.Components;
using Content.Server.Destructible;
using Content.Server.Emp;
using Content.Shared.Backmen.Blob;
using Content.Shared.Backmen.Blob.Components;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.Backmen.Blob.Systems;

public sealed class BlobTileSystem : SharedBlobTileSystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly BlobCoreSystem _blobCoreSystem = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly EmpSystem _empSystem = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly NpcFactionSystem _npcFactionSystem = default!;

    private EntityQuery<BlobCoreComponent> _blobCoreQuery;

    [ValidatePrototypeId<NpcFactionPrototype>]
    private const string BlobFaction = "Blob";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobTileComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BlobTileComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<BlobTileComponent, BlobTileGetPulseEvent>(OnPulsed);
        SubscribeLocalEvent<BlobTileComponent, EntityTerminatingEvent>(OnTerminate);

        _blobCoreQuery = GetEntityQuery<BlobCoreComponent>();
    }

    private void OnMapInit(Entity<BlobTileComponent> ent, ref MapInitEvent args)
    {
        var faction = EnsureComp<NpcFactionMemberComponent>(ent);
        Entity<NpcFactionMemberComponent?> factionEnt = (ent, faction);

        _npcFactionSystem.ClearFactions(factionEnt, false);
        _npcFactionSystem.AddFaction(factionEnt, BlobFaction);

        // make alive - true for npc combat
        EnsureComp<MobStateComponent>(ent);
        EnsureComp<MobThresholdsComponent>(ent);
    }

    private void OnTerminate(EntityUid uid, BlobTileComponent component, EntityTerminatingEvent args)
    {
        if (TerminatingOrDeleted(component.Core))
            return;

        component.Core!.Value.Comp.BlobTiles.Remove(uid);
    }

    private void OnDestruction(EntityUid uid, BlobTileComponent component, DestructionEventArgs args)
    {
        if (TerminatingOrDeleted(component.Core) ||
            !_blobCoreQuery.TryComp(component.Core, out var blobCoreComponent))
            return;

        if (blobCoreComponent.CurrentChem == BlobChemType.ElectromagneticWeb)
        {
            _empSystem.EmpPulse(_transform.GetMapCoordinates(uid), 3f, 50f, 3f);
        }
    }

    private void OnPulsed(EntityUid uid, BlobTileComponent component, BlobTileGetPulseEvent args)
    {
        if (component.Core == null)
            return;

        var core = component.Core.Value;
        var xform = Transform(uid);

        HealTile((uid, component), core);

        if (!args.Handled)
            return;

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
        {
            return;
        }

        var nearNode = _blobCoreSystem.GetNearNode(xform.Coordinates, core.Comp.TilesRadiusLimit);

        if (nearNode == null)
            return;

        var mobTile = _mapSystem.GetTileRef(xform.GridUid.Value, grid, xform.Coordinates);

        var mobAdjacentTiles = new[]
        {
            mobTile.GridIndices.Offset(Direction.East),
            mobTile.GridIndices.Offset(Direction.West),
            mobTile.GridIndices.Offset(Direction.North),
            mobTile.GridIndices.Offset(Direction.South),
        };

        _random.Shuffle(mobAdjacentTiles);

        var localPos = xform.Coordinates.Position;

        var radius = 1.0f;

        var innerTiles = _mapSystem.GetLocalTilesIntersecting(xform.GridUid.Value,
                grid,
                new Box2(localPos + new Vector2(-radius, -radius), localPos + new Vector2(radius, radius)))
            .ToArray();

        foreach (var innerTile in innerTiles)
        {
            if (!mobAdjacentTiles.Contains(innerTile.GridIndices))
            {
                continue;
            }

            foreach (var ent in _mapSystem.GetAnchoredEntities(xform.GridUid.Value, grid, innerTile.GridIndices))
            {
                if (!HasComp<DestructibleComponent>(ent) || !HasComp<ConstructionComponent>(ent))
                    continue;

                DoLunge(uid, ent);
                _damageableSystem.TryChangeDamage(ent, core.Comp.ChemDamageDict[core.Comp.CurrentChem]);
                _audioSystem.PlayPvs(core.Comp.AttackSound, uid, AudioParams.Default);
                args.Handled = true;
                return;
            }

            var spawn = true;
            foreach (var ent in _mapSystem.GetAnchoredEntities(xform.GridUid.Value, grid, innerTile.GridIndices))
            {
                if (!HasComp<BlobTileComponent>(ent))
                    continue;
                spawn = false;
                break;
            }

            if (!spawn)
                continue;

            var location = _mapSystem.ToCoordinates(xform.GridUid.Value, innerTile.GridIndices, grid);

            if (_blobCoreSystem.TransformBlobTile(null,
                    core,
                    nearNode,
                    BlobTileType.Normal,
                    location))
                return;
        }
    }

    private void HealTile(Entity<BlobTileComponent> ent, Entity<BlobCoreComponent> core)
    {
        var healCore = new DamageSpecifier();
        var modifier = 1.0f;

        if (core.Comp.CurrentChem == BlobChemType.RegenerativeMateria)
        {
            modifier = 2.0f;
        }

        foreach (var keyValuePair in ent.Comp.HealthOfPulse.DamageDict)
        {
            healCore.DamageDict.TryAdd(keyValuePair.Key, keyValuePair.Value * modifier);
        }

        _damageableSystem.TryChangeDamage(ent, healCore);
    }

    protected override void TryUpgrade(Entity<BlobTileComponent, BlobUpgradeableTileComponent> target, Entity<BlobCoreComponent> core, EntityUid observer)
    {
        var ev = new BlobTransformTileActionEvent(
            performer: observer,
            target: Transform(target).Coordinates,
            transformFrom: target.Comp1.BlobTileType,
            tileType: target.Comp2.TransformTo);

        RaiseLocalEvent(core, ev);
    }

    public void SwapSpecials(Entity<BlobNodeComponent> from, Entity<BlobNodeComponent> to)
    {
        (from.Comp.BlobFactory, to.Comp.BlobFactory) = (to.Comp.BlobFactory, from.Comp.BlobFactory);
        (from.Comp.BlobResource, to.Comp.BlobResource) = (to.Comp.BlobResource, from.Comp.BlobResource);
        (from.Comp.BlobStorage, to.Comp.BlobStorage) = (to.Comp.BlobStorage, from.Comp.BlobStorage);
        (from.Comp.BlobTurret, to.Comp.BlobTurret) = (to.Comp.BlobTurret, from.Comp.BlobTurret);
        Dirty(from);
        Dirty(to);
    }

    public bool IsEmptySpecial(Entity<BlobNodeComponent> node, BlobTileType tile)
    {
        return tile switch
        {
            BlobTileType.Factory => node.Comp.BlobFactory == null || TerminatingOrDeleted(node.Comp.BlobFactory),
            BlobTileType.Resource => node.Comp.BlobResource == null || TerminatingOrDeleted(node.Comp.BlobResource),
            BlobTileType.Storage => node.Comp.BlobStorage == null || TerminatingOrDeleted(node.Comp.BlobStorage),
            BlobTileType.Turret => node.Comp.BlobTurret == null || TerminatingOrDeleted(node.Comp.BlobTurret),
            _ => false
        };
    }

    public void DoLunge(EntityUid from, EntityUid target)
    {
        if(!TransformQuery.TryComp(from, out var userXform))
            return;

        var targetPos = _transform.GetWorldPosition(target);
        var localPos = Vector2.Transform(targetPos, _transform.GetInvWorldMatrix(userXform));
        localPos = userXform.LocalRotation.RotateVec(localPos);

        RaiseNetworkEvent(new BlobAttackEvent(GetNetEntity(from), GetNetEntity(target), localPos), Filter.Pvs(from));
    }
}
