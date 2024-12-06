using System.Numerics;
using Content.Shared.Body.Systems;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Targeting;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared.Projectiles;

public abstract partial class SharedProjectileSystem : EntitySystem
{
    public const string ProjectileFixture = "projectile";

    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ProjectileComponent, PreventCollideEvent>(PreventCollision);
        SubscribeLocalEvent<EmbeddableProjectileComponent, ProjectileHitEvent>(OnEmbedProjectileHit);
        SubscribeLocalEvent<EmbeddableProjectileComponent, ThrowDoHitEvent>(OnEmbedThrowDoHit);
        SubscribeLocalEvent<EmbeddableProjectileComponent, ActivateInWorldEvent>(OnEmbedActivate);
        SubscribeLocalEvent<EmbeddableProjectileComponent, RemoveEmbeddedProjectileEvent>(OnEmbedRemove);
        SubscribeLocalEvent<EmbeddableProjectileComponent, AttemptPacifiedThrowEvent>(OnAttemptPacifiedThrow);
        SubscribeLocalEvent<EmbeddableProjectileComponent, ExaminedEvent>(OnExamined);
    }

    // TODO: rename Embedded to Target in every context
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<EmbeddableProjectileComponent>();
        var curTime = _timing.CurTime;

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.AutoRemoveTime == null || comp.AutoRemoveTime > curTime)
                continue;

            if (comp.Target is {} targetUid)
                _popup.PopupClient(Loc.GetString("throwing-embed-falloff", ("item", uid)), targetUid, targetUid);

            RemoveEmbed(uid, comp);
        }
    }

    private void OnEmbedActivate(EntityUid uid, EmbeddableProjectileComponent component, ActivateInWorldEvent args)
    {
        // Nuh uh
        if (component.RemovalTime == null)
            return;

        if (args.Handled || !args.Complex || !TryComp<PhysicsComponent>(uid, out var physics) || physics.BodyType != BodyType.Static)
            return;

        args.Handled = true;

        if (component.Target is {} targetUid)
            _popup.PopupClient(Loc.GetString("throwing-embed-remove-alert-owner", ("item", uid), ("other", args.User)),
                args.User, targetUid);

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, component.RemovalTime.Value,
            new RemoveEmbeddedProjectileEvent(), eventTarget: uid, target: uid)
        {
            DistanceThreshold = SharedInteractionSystem.InteractionRange,
            BreakOnUserMove = true,
            BreakOnTargetMove = true,
            NeedHand = true,
        });
    }

    private void OnEmbedRemove(EntityUid uid, EmbeddableProjectileComponent component, RemoveEmbeddedProjectileEvent args)
    {
        if (args.Cancelled)
            return;

        RemoveEmbed(uid, component, args.User);
    }

    private void RemoveEmbed(EntityUid uid, EmbeddableProjectileComponent component, EntityUid? remover = null)
    {
        component.AutoRemoveTime = null;
        component.Target = null;
        component.TargetBodyPart = null;

        var ev = new RemoveEmbedEvent(remover);
        RaiseLocalEvent(uid, ref ev);

        // Whacky prediction issues.
        if (_netManager.IsClient)
            return;

        if (component.DeleteOnRemove)
        {
            QueueDel(uid);
            return;
        }

        var xform = Transform(uid);
        TryComp<PhysicsComponent>(uid, out var physics);
        _physics.SetBodyType(uid, BodyType.Dynamic, body: physics, xform: xform);
        _transform.AttachToGridOrMap(uid, xform);

        // Reset whether the projectile has damaged anything if it successfully was removed
        if (TryComp<ProjectileComponent>(uid, out var projectile))
        {
            projectile.Shooter = null;
            projectile.Weapon = null;
            projectile.DamagedEntity = false;
        }

        // Land it just coz uhhh yeah
        var landEv = new LandEvent(remover, true);
        RaiseLocalEvent(uid, ref landEv);
        _physics.WakeBody(uid, body: physics);

        // try place it in the user's hand
        if (remover is {} removerUid)
            _hands.TryPickupAnyHand(removerUid, uid);
    }

    private void OnEmbedThrowDoHit(EntityUid uid, EmbeddableProjectileComponent component, ThrowDoHitEvent args)
    {
        if (!component.EmbedOnThrow ||
            HasComp<ThrownItemImmuneComponent>(args.Target))
            return;

        Embed(uid, args.Target, null, component, args.TargetPart);
    }

    private void OnEmbedProjectileHit(EntityUid uid, EmbeddableProjectileComponent component, ref ProjectileHitEvent args)
    {
        Embed(uid, args.Target, args.Shooter, component);

        // Raise a specific event for projectiles.
        if (TryComp(uid, out ProjectileComponent? projectile))
        {
            var ev = new ProjectileEmbedEvent(projectile.Shooter!.Value, projectile.Weapon!.Value, args.Target);
            RaiseLocalEvent(uid, ref ev);
        }
    }

    private void Embed(EntityUid uid, EntityUid target, EntityUid? user, EmbeddableProjectileComponent component, TargetBodyPart? targetPart = null)
    {
        TryComp<PhysicsComponent>(uid, out var physics);
        _physics.SetLinearVelocity(uid, Vector2.Zero, body: physics);
        _physics.SetBodyType(uid, BodyType.Static, body: physics);
        var xform = Transform(uid);
        _transform.SetParent(uid, xform, target);

        if (component.Offset != Vector2.Zero)
        {
            _transform.SetLocalPosition(uid, xform.LocalPosition + xform.LocalRotation.RotateVec(component.Offset),
                xform);
        }

        _audio.PlayPredicted(component.Sound, uid, null);

        component.TargetBodyPart = targetPart;
        var ev = new EmbedEvent(user, target, targetPart);
        RaiseLocalEvent(uid, ref ev);

        if (component.AutoRemoveDuration != 0)
            component.AutoRemoveTime = _timing.CurTime + TimeSpan.FromSeconds(component.AutoRemoveDuration);

        component.Target = target;

        Dirty(uid, component);
    }

    private void PreventCollision(EntityUid uid, ProjectileComponent component, ref PreventCollideEvent args)
    {
        if (component.IgnoreShooter && (args.OtherEntity == component.Shooter || args.OtherEntity == component.Weapon))
        {
            args.Cancelled = true;
        }
    }

    public void SetShooter(EntityUid id, ProjectileComponent component, EntityUid shooterId)
    {
        if (component.Shooter == shooterId)
            return;

        component.Shooter = shooterId;
        Dirty(id, component);
    }

    /// <summary>
    /// Prevent players with the Pacified status effect from throwing embeddable projectiles.
    /// </summary>
    private void OnAttemptPacifiedThrow(Entity<EmbeddableProjectileComponent> ent, ref AttemptPacifiedThrowEvent args)
    {
        args.Cancel("pacified-cannot-throw-embed");
    }

    private void OnExamined(EntityUid uid, EmbeddableProjectileComponent component, ExaminedEvent args)
    {
        if (!(component.Target is {} target))
            return;

        var targetIdentity = Identity.Entity(target, EntityManager);

        var loc = component.TargetBodyPart == null
            ? Loc.GetString("throwing-examine-embedded",
            ("embedded", uid),
            ("target", targetIdentity))
            : Loc.GetString("throwing-examine-embedded-part",
            ("embedded", uid),
            ("target", targetIdentity),
            ("targetPart", Loc.GetString($"body-part-{component.TargetBodyPart.ToString()}")));

        args.PushMarkup(loc);
    }

    [Serializable, NetSerializable]
    private sealed partial class RemoveEmbeddedProjectileEvent : DoAfterEvent
    {
        public override DoAfterEvent Clone() => this;
    }
}

[Serializable, NetSerializable]
public sealed class ImpactEffectEvent : EntityEventArgs
{
    public string Prototype;
    public NetCoordinates Coordinates;

    public ImpactEffectEvent(string prototype, NetCoordinates coordinates)
    {
        Prototype = prototype;
        Coordinates = coordinates;
    }
}

/// <summary>
/// Raised when an entity is just about to be hit with a projectile but can reflect it
/// </summary>
[ByRefEvent]
public record struct ProjectileReflectAttemptEvent(EntityUid ProjUid, ProjectileComponent Component, bool Cancelled);

/// <summary>
/// Raised when a projectile hits an entity
/// </summary>
[ByRefEvent]
public record struct ProjectileHitEvent(DamageSpecifier Damage, EntityUid Target, EntityUid? Shooter = null);
