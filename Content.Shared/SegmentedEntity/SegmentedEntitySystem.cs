using Robust.Shared.Physics;
using Content.Shared.Damage;
using Content.Shared.Explosion;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Tag;
using Content.Shared.Storage.Components;
using Robust.Shared.Physics.Events;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Teleportation.Components;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Components;
using System.Numerics;
using System.Net;
using Robust.Shared.Network;
using System.Net.WebSockets;

namespace Content.Shared.SegmentedEntity
{
    public sealed partial class LamiaSystem : EntitySystem
    {
        [Dependency] private readonly TagSystem _tagSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoid = default!;
        [Dependency] private readonly SharedJointSystem _jointSystem = default!;
        [Dependency] private readonly INetManager _net = default!;

        Queue<(SegmentedEntitySegmentComponent segment, EntityUid lamia)> _segments = new();

        [ValidatePrototypeId<TagPrototype>]
        private const string LamiaHardsuitTag = "AllowLamiaHardsuit";
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
            SubscribeLocalEvent<SegmentedEntityComponent, EntGotRemovedFromContainerMessage>(OnRemovedFromContainer);
            SubscribeLocalEvent<SegmentedEntityComponent, EntParentChangedMessage>(OnParentChanged);

            //Child subscriptions
            SubscribeLocalEvent<SegmentedEntitySegmentComponent, InsertIntoEntityStorageAttemptEvent>(OnSegmentStorageInsertAttempt);
            SubscribeLocalEvent<SegmentedEntitySegmentComponent, GetExplosionResistanceEvent>(OnSnekBoom);
            SubscribeLocalEvent<SegmentedEntitySegmentComponent, DamageChangedEvent>(HandleDamageTransfer);
            SubscribeLocalEvent<SegmentedEntitySegmentComponent, DamageModifyEvent>(HandleSegmentDamage);
            SubscribeLocalEvent<SegmentedEntitySegmentComponent, SegmentSpawnedEvent>(OnSegmentSpawned);
        }
        public override void Update(float frameTime)
        {
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

                var ev = new SegmentSpawnedEvent(segment.lamia);
                RaiseLocalEvent(segmentUid, ev, false);

                if (segment.segment.SegmentNumber == 1)
                {
                    Transform(segmentUid).Coordinates = Transform(attachedUid).Coordinates;
                    var revoluteJoint = _jointSystem.CreateWeldJoint(attachedUid, segmentUid, id: "Segment" + segment.segment.SegmentNumber + segment.segment.Lamia);
                    revoluteJoint.CollideConnected = false;
                }
                if (segment.segment.SegmentNumber <= segment.segment.MaxSegments)
                    Transform(segmentUid).Coordinates = Transform(attachedUid).Coordinates.Offset(new Vector2(0, segment.segment.OffsetSwitching));
                else
                    Transform(segmentUid).Coordinates = Transform(attachedUid).Coordinates.Offset(new Vector2(0, segment.segment.OffsetSwitching));

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

            foreach (var segment in component.Segments)
            {
                QueueDel(segment);
            }

            component.Segments.Clear();
        }

        private void SegmentSelfTest(EntityUid uid, SegmentedEntityComponent component)
        {

        }

        private void OnJointRemoved(EntityUid uid, SegmentedEntityComponent component, JointRemovedEvent args)
        {
            if (!component.Segments.Contains(args.OtherEntity))
                return;

            RespawnSegments(uid, component);
        }

        private void OnRemovedFromContainer(EntityUid uid, SegmentedEntityComponent component, EntGotRemovedFromContainerMessage args)
        {
            RespawnSegments(uid, component);
        }

        private void DeleteSegments(SegmentedEntityComponent component)
        {
            if (_net.IsClient)
                return; //Client is not allowed to predict QueueDel, it'll throw an error(but won't crash in Release build)

            foreach (var segment in component.Segments)
                QueueDel(segment);

            component.Segments.Clear();
        }

        /// <summary>
        ///     Public call for a SegmentedEntity to reset their tail completely.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="component"></param>
        public void RespawnSegments(EntityUid uid, SegmentedEntityComponent component)
        {
            DeleteSegments(component);
            SpawnSegments(uid, component);
        }

        private void SpawnSegments(EntityUid uid, SegmentedEntityComponent component)
        {
            if (_net.IsClient)
                return; //Client is not allowed to spawn entities. It won't throw an error, but it'll make fake client entities.

            //Segmented Entities are potentially not humanoids, they could for instance be a giant space worm.
            var humanoidFactor = TryComp<HumanoidAppearanceComponent>(uid, out var humanoid) ? (humanoid.Height + humanoid.Width) / 2 : 1;

            int i = 1;
            var addTo = uid;
            while (i <= component.NumberOfSegments + 1)
            {
                var segment = AddSegment(addTo, uid, component, i, humanoidFactor);
                addTo = segment;
                i++;
            }
        }

        private EntityUid AddSegment(EntityUid segmentuid, EntityUid parentuid, SegmentedEntityComponent segmentedComponent, int segmentNumber, float humanoidFactor)
        {

            float taperConstant = segmentedComponent.NumberOfSegments - segmentedComponent.TaperOffset;
            EntityUid segment;
            if (segmentNumber == 1)
                segment = EntityManager.SpawnEntity(segmentedComponent.InitialSegmentId, Transform(segmentuid).Coordinates);
            else
                segment = EntityManager.SpawnEntity(segmentedComponent.SegmentId, Transform(segmentuid).Coordinates);

            var segmentComponent = EnsureComp<SegmentedEntitySegmentComponent>(segment);

            segmentComponent.Lamia = parentuid;
            segmentComponent.AttachedToUid = segmentuid;
            segmentComponent.DamageModifierConstant = segmentedComponent.NumberOfSegments * segmentedComponent.DamageModifierOffset;
            float damageModifyCoefficient = segmentComponent.DamageModifierConstant / segmentedComponent.NumberOfSegments;
            segmentComponent.DamageModifyFactor = segmentComponent.DamageModifierConstant * damageModifyCoefficient;
            segmentComponent.ExplosiveModifyFactor = 1 / segmentComponent.DamageModifyFactor / (segmentedComponent.NumberOfSegments * segmentedComponent.ExplosiveModifierOffset);
            segmentComponent.SegmentNumber = segmentNumber;
            segmentComponent.Owner = segment;

            if (segmentedComponent.UseTaperSystem == true)
            {
                if (segmentNumber >= taperConstant)
                {
                    segmentComponent.OffsetSwitching = segmentedComponent.StaticOffset
                    * humanoidFactor
                    * MathF.Pow(segmentedComponent.OffsetConstant, segmentNumber - taperConstant);

                    segmentComponent.ScaleFactor = segmentedComponent.StaticScale
                    * humanoidFactor
                    * MathF.Pow(1f / segmentedComponent.OffsetConstant, segmentNumber - taperConstant);
                }
                if (segmentNumber < taperConstant)
                {
                    segmentComponent.OffsetSwitching = segmentedComponent.StaticOffset * humanoidFactor;
                    segmentComponent.ScaleFactor = segmentedComponent.StaticScale * humanoidFactor;
                }
            }
            else
            {
                segmentComponent.OffsetSwitching = segmentedComponent.StaticOffset * humanoidFactor;
                segmentComponent.ScaleFactor = segmentedComponent.StaticScale * humanoidFactor;
            }

            // We invert the Y axis offset on every odd numbered tail so that the segmented entity spawns in a neat pile
            // Rather than stretching across 5 to 10 vertical tiles, and potentially getting trapped in a wall
            if (segmentNumber % 2 != 0)
            {
                segmentComponent.OffsetSwitching *= -1;
            }

            EnsureComp<PortalExemptComponent>(segment); //Not temporary, segments must never be allowed to go through portals for physics limitation reasons
            _segments.Enqueue((segmentComponent, parentuid));
            segmentedComponent.Segments.Add(segment);
            return segment;
        }

        /// <summary>
        /// Handles transferring marking selections to the tail segments. Every tail marking must be repeated 2 times in order for this script to work.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="component"></param>
        /// <param name="args"></param>
        // TODO: Please for the love of god don't make me write a test to validate that every marking also has its matching segment states.
        // Future contributors will just find out when their game crashes because they didn't make a marking-segment.
        private void OnSegmentSpawned(EntityUid uid, SegmentedEntitySegmentComponent component, SegmentSpawnedEvent args)
        {
            component.Lamia = args.Lamia;

            if (!TryComp<HumanoidAppearanceComponent>(uid, out var species)) return;
            if (!TryComp<HumanoidAppearanceComponent>(args.Lamia, out var humanoid)) return;
            if (!TryComp<AppearanceComponent>(uid, out var appearance)) return;

            _appearance.SetData(uid, ScaleVisuals.Scale, component.ScaleFactor, appearance);

            if (humanoid.MarkingSet.TryGetCategory(MarkingCategories.Tail, out var tailMarkings))
            {
                foreach (var markings in tailMarkings)
                {
                    var segmentId = species.Species;
                    var markingId = markings.MarkingId;
                    string segmentmarking = $"{markingId}-{segmentId}";
                    _humanoid.AddMarking(uid, segmentmarking, markings.MarkingColors);
                }
            }
        }

        private void HandleSegmentDamage(EntityUid uid, SegmentedEntitySegmentComponent component, DamageModifyEvent args)
        {
            if (args.Origin == component.Lamia)
                args.Damage *= 0;
            args.Damage = args.Damage / component.DamageModifyFactor;
        }
        private void HandleDamageTransfer(EntityUid uid, SegmentedEntitySegmentComponent component, DamageChangedEvent args)
        {
            if (args.DamageDelta == null) return;
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
            if (!TryComp<ClothingComponent>(args.Equipment, out var clothing)) return;
            if (args.Slot == "outerClothing" && _tagSystem.HasTag(args.Equipment, LamiaHardsuitTag))
            {
                foreach (var uid in component.Segments)
                {
                    if (!TryComp<AppearanceComponent>(uid, out var appearance)) return;
                    _appearance.SetData(uid, SegmentedEntitySegmentVisualLayers.Armor, true, appearance);
                    if (clothing.RsiPath == null) return;
                    _appearance.SetData(uid, SegmentedEntitySegmentVisualLayers.ArmorRsi, clothing.RsiPath, appearance);
                }
            }
        }

        private void OnSnekBoom(EntityUid uid, SegmentedEntitySegmentComponent component, ref GetExplosionResistanceEvent args)
        {
            args.DamageCoefficient = component.ExplosiveModifyFactor;
        }

        private void OnDidUnequipEvent(EntityUid equipee, SegmentedEntityComponent component, DidUnequipEvent args)
        {
            if (args.Slot == "outerClothing" && _tagSystem.HasTag(args.Equipment, LamiaHardsuitTag))
            {
                foreach (var uid in component.Segments)
                {
                    if (!TryComp<AppearanceComponent>(uid, out var appearance)) return;
                    _appearance.SetData(uid, SegmentedEntitySegmentVisualLayers.Armor, false, appearance);
                }
            }
        }

        private void OnShootHitscan(EntityUid uid, SegmentedEntityComponent component, ref HitScanAfterRayCastEvent args)
        {
            if (args.RayCastResults == null) return;

            var entityList = new List<RayCastResults>();
            foreach (var entity in args.RayCastResults)
            {
                if (!component.Segments.Contains(entity.HitEntity))
                    entityList.Add(entity);
            }
            args.RayCastResults = entityList;
        }

        private void OnParentChanged(EntityUid uid, SegmentedEntityComponent component, ref EntParentChangedMessage args)
        {
            if (Transform(uid).MapID != args.OldMapId)
                return;

            RespawnSegments(uid, component);
        }
    }
}
