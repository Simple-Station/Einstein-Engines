using Robust.Shared.Physics;
using Content.Shared.Damage;
using Content.Shared.Explosion;
using Content.Shared.Humanoid;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Tag;
using Content.Shared.Storage.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Teleportation.Components;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using System.Numerics;
using Robust.Shared.Network;

namespace Content.Shared.SegmentedEntity;

public sealed partial class LamiaSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedJointSystem _jointSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private Queue<(SegmentedEntitySegmentComponent segment, EntityUid lamia)> _segments = new();

    private ProtoId<TagPrototype> _lamiaHardsuitTag = "AllowLamiaHardsuit";

    public override void Initialize()
    {
        base.Initialize();
        //Parent subscriptions
        SubscribeLocalEvent<SegmentedEntityComponent, HitScanAfterRayCastEvent>(OnShootHitscan);
        SubscribeLocalEvent<SegmentedEntityComponent, InsertIntoEntityStorageAttemptEvent>(OnLamiaStorageInsertAttempt);
        SubscribeLocalEvent<SegmentedEntityComponent, DidEquipEvent>(OnDidEquipEvent);
        SubscribeLocalEvent<SegmentedEntityComponent, DidUnequipEvent>(OnDidUnequipEvent);
        SubscribeLocalEvent<SegmentedEntityComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SegmentedEntityComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SegmentedEntityComponent, JointRemovedEvent>(OnJointRemoved);
        SubscribeLocalEvent<SegmentedEntityComponent, EntParentChangedMessage>(OnParentChanged);
        SubscribeLocalEvent<SegmentedEntityComponent, StoreMobInItemContainerAttemptEvent>(OnStoreSnekAttempt);

        //Child subscriptions
        SubscribeLocalEvent<SegmentedEntitySegmentComponent, InsertIntoEntityStorageAttemptEvent>(OnSegmentStorageInsertAttempt);
        SubscribeLocalEvent<SegmentedEntitySegmentComponent, GetExplosionResistanceEvent>(OnSnekBoom);
        SubscribeLocalEvent<SegmentedEntitySegmentComponent, DamageChangedEvent>(HandleDamageTransfer);
        SubscribeLocalEvent<SegmentedEntitySegmentComponent, DamageModifyEvent>(HandleSegmentDamage);
    }
    public override void Update(float frameTime)
    {
        //I HATE THIS, SO MUCH. I AM FORCED TO DEAL WITH THIS MONSTROSITY. PLEASE. SEND HELP.
        base.Update(frameTime);
        foreach (var segment in _segments)
        {
            var segmentUid = segment.segment.Owner;
            var attachedUid = segment.segment.AttachedToUid;
            if (!Exists(segmentUid) || !Exists(attachedUid)
            || MetaData(segmentUid).EntityLifeStage > EntityLifeStage.MapInitialized
            || MetaData(attachedUid).EntityLifeStage > EntityLifeStage.MapInitialized
            || Transform(segmentUid).MapID == MapId.Nullspace
            || Transform(attachedUid).MapID == MapId.Nullspace)
                continue;

            EnsureComp<PhysicsComponent>(segmentUid);
            EnsureComp<PhysicsComponent>(attachedUid); // Hello I hate tests

            // This is currently HERE and not somewhere more sane like OnInit because HumanoidAppearanceComponent is for whatever
            // ungodly reason not initialized when ComponentStartup is called. Kill me.
            var humanoidFactor = TryComp<HumanoidAppearanceComponent>(segment.segment.Lamia, out var humanoid) ? (humanoid.Height + humanoid.Width) / 2 : 1;

            var ev = new SegmentSpawnedEvent(segment.lamia);
            RaiseLocalEvent(segmentUid, ev, false);

            if (segment.segment.SegmentNumber == 1)
            {
                _transform.SetCoordinates(segmentUid, Transform(attachedUid).Coordinates);
                var revoluteJoint = _jointSystem.CreateWeldJoint(attachedUid, segmentUid, id: "Segment" + segment.segment.SegmentNumber + segment.segment.Lamia);
                revoluteJoint.CollideConnected = false;
            }
            if (segment.segment.SegmentNumber <= segment.segment.MaxSegments)
                _transform.SetCoordinates(segmentUid, Transform(attachedUid).Coordinates.Offset(new Vector2(0, segment.segment.OffsetSwitching * humanoidFactor)));
            else
                _transform.SetCoordinates(segmentUid, Transform(attachedUid).Coordinates.Offset(new Vector2(0, segment.segment.OffsetSwitching * humanoidFactor)));

            var joint = _jointSystem.CreateDistanceJoint(attachedUid, segmentUid, id: ("Segment" + segment.segment.SegmentNumber + segment.segment.Lamia));
            joint.CollideConnected = false;
            joint.Stiffness = 0.2f;
        }
        _segments.Clear();
    }
    private void OnInit(EntityUid uid, SegmentedEntityComponent component, ComponentInit args)
    {
        EnsureComp<PortalExemptComponent>(uid); //Temporary, remove when Portal handling is added
        Math.Clamp(component.NumberOfSegments, 2, 18);
        Math.Clamp(component.TaperOffset, 1, component.NumberOfSegments - 1);
        SpawnSegments(uid, component);
    }

    private void OnShutdown(EntityUid uid, SegmentedEntityComponent component, ComponentShutdown args)
    {
        if (_net.IsClient)
            return;

        DeleteSegments(component);
    }

    /// <summary>
    ///     TODO: Full Self-Test function that intelligently checks the status of where everything is, and calls whatever
    ///     functions are appropriate
    /// </summary>
    public void SegmentSelfTest(EntityUid uid, SegmentedEntityComponent component)
    {

    }

    /// <summary>
    ///     TODO: Function that ensures clothing visuals, to be called anytime the tail is reset
    /// </summary>
    private void EnsureSnekSock(EntityUid uid, SegmentedEntityComponent segment)
    {

    }

    public void OnStoreSnekAttempt(EntityUid uid, SegmentedEntityComponent comp, ref StoreMobInItemContainerAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnJointRemoved(EntityUid uid, SegmentedEntityComponent component, JointRemovedEvent args)
    {
        if (!component.Segments.Contains(GetNetEntity(args.OtherEntity)))
            return;

        DeleteSegments(component);
    }

    private void DeleteSegments(SegmentedEntityComponent component)
    {
        if (_net.IsClient)
            return; //Client is not allowed to predict QueueDel, it'll throw an error(but won't crash in Release build)

        foreach (var segment in component.Segments)
            QueueDel(GetEntity(segment));

        component.Segments.Clear();
    }

    /// <summary>
    ///     Public call for a SegmentedEntity to reset their tail completely.
    /// </summary>
    public void RespawnSegments(EntityUid uid, SegmentedEntityComponent component)
    {
        DeleteSegments(component);
        SpawnSegments(uid, component);
    }

    private void SpawnSegments(EntityUid uid, SegmentedEntityComponent component)
    {
        if (_net.IsClient)
            return; //Client is not allowed to spawn entities. It won't throw an error, but it'll make fake client entities.

        int i = 1;
        var addTo = uid;
        while (i <= component.NumberOfSegments + 1)
        {
            var segment = AddSegment(addTo, uid, component, i);
            addTo = segment;
            i++;
        }

        Dirty(uid, component);
    }

    private EntityUid AddSegment(EntityUid segmentuid, EntityUid parentuid, SegmentedEntityComponent segmentedComponent, int segmentNumber)
    {
        float taperConstant = segmentedComponent.NumberOfSegments - segmentedComponent.TaperOffset;
        EntityUid segment = EntityManager.SpawnEntity(segmentNumber == 1
            ? segmentedComponent.InitialSegmentId
            : segmentedComponent.SegmentId, Transform(segmentuid).Coordinates);

        var segmentComponent = EnsureComp<SegmentedEntitySegmentComponent>(segment);

        segmentComponent.Lamia = parentuid;
        segmentComponent.AttachedToUid = segmentuid;
        segmentComponent.DamageModifierConstant = segmentedComponent.NumberOfSegments * segmentedComponent.DamageModifierOffset;
        float damageModifyCoefficient = segmentComponent.DamageModifierConstant / segmentedComponent.NumberOfSegments;
        segmentComponent.DamageModifyFactor = segmentComponent.DamageModifierConstant * damageModifyCoefficient;
        segmentComponent.ExplosiveModifyFactor = 1 / segmentComponent.DamageModifyFactor / (segmentedComponent.NumberOfSegments * segmentedComponent.ExplosiveModifierOffset);
        segmentComponent.SegmentNumber = segmentNumber;

        if (segmentedComponent.UseTaperSystem)
        {
            if (segmentNumber >= taperConstant)
            {
                segmentComponent.OffsetSwitching = segmentedComponent.StaticOffset
                * MathF.Pow(segmentedComponent.OffsetConstant, segmentNumber - taperConstant);

                segmentComponent.ScaleFactor = segmentedComponent.StaticScale
                * MathF.Pow(1f / segmentedComponent.OffsetConstant, segmentNumber - taperConstant);
            }
            if (segmentNumber < taperConstant)
            {
                segmentComponent.OffsetSwitching = segmentedComponent.StaticOffset;
                segmentComponent.ScaleFactor = segmentedComponent.StaticScale;
            }
        }
        else
        {
            segmentComponent.OffsetSwitching = segmentedComponent.StaticOffset;
            segmentComponent.ScaleFactor = segmentedComponent.StaticScale;
        }

        // We invert the Y axis offset on every odd numbered tail so that the segmented entity spawns in a neat pile
        // Rather than stretching across 5 to 10 vertical tiles, and potentially getting trapped in a wall
        if (segmentNumber % 2 != 0)
        {
            segmentComponent.OffsetSwitching *= -1;
        }

        EnsureComp<PortalExemptComponent>(segment); //Not temporary, segments must never be allowed to go through portals for physics limitation reasons
        _segments.Enqueue((segmentComponent, parentuid));
        segmentedComponent.Segments.Add(GetNetEntity(segment));
        return segment;
    }

    private void HandleSegmentDamage(EntityUid uid, SegmentedEntitySegmentComponent component, DamageModifyEvent args)
    {
        if (args.Origin == component.Lamia)
            args.Damage *= 0;
        args.Damage = args.Damage / component.DamageModifyFactor;
    }

    private void HandleDamageTransfer(EntityUid uid, SegmentedEntitySegmentComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null)
            return;

        _damageableSystem.TryChangeDamage(component.Lamia, args.DamageDelta);
    }

    private void OnLamiaStorageInsertAttempt(EntityUid uid, SegmentedEntityComponent comp, ref InsertIntoEntityStorageAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnSegmentStorageInsertAttempt(EntityUid uid, SegmentedEntitySegmentComponent comp, ref InsertIntoEntityStorageAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnDidEquipEvent(EntityUid equipee, SegmentedEntityComponent component, DidEquipEvent args)
    {
        if (!TryComp<ClothingComponent>(args.Equipment, out var clothing)
            || args.Slot != "outerClothing"
            || !_tagSystem.HasTag(args.Equipment, _lamiaHardsuitTag))
            return;

        // TODO: Switch segment sprite
    }

    private void OnSnekBoom(EntityUid uid, SegmentedEntitySegmentComponent component, ref GetExplosionResistanceEvent args)
    {
        args.DamageCoefficient = component.ExplosiveModifyFactor;
    }

    private void OnDidUnequipEvent(EntityUid equipee, SegmentedEntityComponent component, DidUnequipEvent args)
    {
        if (args.Slot == "outerClothing" && _tagSystem.HasTag(args.Equipment, _lamiaHardsuitTag))
        {
            // TODO: Revert to default segment sprite
        }
    }

    private void OnShootHitscan(EntityUid uid, SegmentedEntityComponent component, ref HitScanAfterRayCastEvent args)
    {
        if (args.RayCastResults == null)
            return;

        var entityList = new List<RayCastResults>();
        foreach (var entity in args.RayCastResults)
        {
            if (!component.Segments.Contains(GetNetEntity(entity.HitEntity)))
                entityList.Add(entity);
        }

        args.RayCastResults = entityList;
    }

    private void OnParentChanged(EntityUid uid, SegmentedEntityComponent component, ref EntParentChangedMessage args)
    {
        //If the change was NOT to a different map
        //if (args.OldMapId == args.Transform.MapUid)
        //    RespawnSegments(uid, component);
    }
}
