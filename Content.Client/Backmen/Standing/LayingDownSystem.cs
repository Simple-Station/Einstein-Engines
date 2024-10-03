using Content.Shared.ActionBlocker;
using Content.Shared.Backmen.CCVar;
using Content.Shared.Backmen.Standing;
using Content.Shared.Buckle;
using Content.Shared.Rotation;
using Content.Shared.Standing;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Client.Backmen.Standing;

public sealed class LayingDownSystem : SharedLayingDownSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LayingDownComponent, MoveEvent>(OnMovementInput);
        SubscribeAllEvent<DrawDownedEvent>(OnDowned);
        SubscribeAllEvent<DrawUpEvent>(OnUp);
        SubscribeLocalEvent<LayingDownComponent, StoodEvent>(OnStood);

        _cfg.OnValueChanged(CCVars.AutoGetUp, b => _autoGetUp = b, true);

        //SubscribeNetworkEvent<CheckAutoGetUpEvent>(OnCheckAutoGetUp);
    }

    private void OnUp(DrawUpEvent args)
    {
        if(!TryGetEntity(args.Uid, out var uid))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite)
            || !TryComp<LayingDownComponent>(uid, out var component)
            || !component.OriginalDrawDepth.HasValue)
            return;

        sprite.DrawDepth = component.OriginalDrawDepth.Value;
    }

    private void OnDowned(DrawDownedEvent args)
    {
        if(!TryGetEntity(args.Uid, out var uid))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite)
            || !TryComp<LayingDownComponent>(uid, out var component))
            return;

        if (!component.OriginalDrawDepth.HasValue)
            component.OriginalDrawDepth = sprite.DrawDepth;

        sprite.DrawDepth = (int) Content.Shared.DrawDepth.DrawDepth.SmallMobs;
    }

    private void OnStood(EntityUid uid, LayingDownComponent component, StoodEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite)
            || !component.OriginalDrawDepth.HasValue)
            return;
        sprite.DrawDepth = component.OriginalDrawDepth.Value;
    }

    private bool _autoGetUp;

    protected override bool GetAutoGetUp(Entity<LayingDownComponent> ent, ICommonSession session)
    {
        return _autoGetUp;
    }

    private void OnMovementInput(EntityUid uid, LayingDownComponent component, MoveEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;
        if(!_standing.IsDown(uid) || _animation.HasRunningAnimation(uid, "rotate") || _buckle.IsBuckled(uid))
            return;
        if(TerminatingOrDeleted(uid))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite)
            || !TryComp<RotationVisualsComponent>(uid, out var rotationVisuals))
        {
            return;
        }

        ProcessVisuals((uid, Transform(uid), sprite, rotationVisuals));
    }

    private void ProcessVisuals(Entity<TransformComponent, SpriteComponent?, RotationVisualsComponent> entity)
    {
        var rotation = entity.Comp1.LocalRotation + (_eyeManager.CurrentEye.Rotation - (entity.Comp1.LocalRotation - _transform.GetWorldRotation(entity.Comp1)));

        if (rotation.GetDir() is Direction.SouthEast or Direction.East or Direction.NorthEast or Direction.North)
        {
            entity.Comp3.HorizontalRotation = Angle.FromDegrees(270);
            if(entity.Comp2 != null)
                entity.Comp2.Rotation = Angle.FromDegrees(270);
            return;
        }

        entity.Comp3.HorizontalRotation = Angle.FromDegrees(90);
        if(entity.Comp2 != null)
            entity.Comp2.Rotation = Angle.FromDegrees(90);
    }

    public override void AutoGetUp(Entity<LayingDownComponent> ent)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if(TerminatingOrDeleted(ent))
            return;

        var transform = Transform(ent);

        if (!TryComp<RotationVisualsComponent>(ent, out var rotationVisuals))
            return;

        ProcessVisuals((ent.Owner, transform, null, rotationVisuals));
    }

    /*
    private void OnCheckAutoGetUp(CheckAutoGetUpEvent ev, EntitySessionEventArgs args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var uid = GetEntity(ev.User);

        if (!TryComp<TransformComponent>(uid, out var transform) || !TryComp<RotationVisualsComponent>(uid, out var rotationVisuals))
            return;

        var rotation = transform.LocalRotation + (_eyeManager.CurrentEye.Rotation - (transform.LocalRotation - transform.WorldRotation));

        if (rotation.GetDir() is Direction.SouthEast or Direction.East or Direction.NorthEast or Direction.North)
        {
            rotationVisuals.HorizontalRotation = Angle.FromDegrees(270);
            return;
        }

        rotationVisuals.HorizontalRotation = Angle.FromDegrees(90);
    }*/
}
