using Content.Shared.ActionBlocker;
using Content.Shared.Rotation;
using Content.Shared.Standing;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Timing;

namespace Content.Client.Standing;

public sealed class LayingDownSystem : SharedLayingDownSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LayingDownComponent, MoveEvent>(OnMovementInput);

        SubscribeNetworkEvent<CheckAutoGetUpEvent>(OnCheckAutoGetUp);
    }

    private void OnMovementInput(EntityUid uid, LayingDownComponent component, MoveEvent args)
    {
        if (!_timing.IsFirstTimePredicted
            || !_actionBlocker.CanMove(uid)
            || _animation.HasRunningAnimation(uid, "rotate")
            || !TryComp<TransformComponent>(uid, out var transform)
            || !TryComp<SpriteComponent>(uid, out var sprite)
            || !TryComp<RotationVisualsComponent>(uid, out var rotationVisuals))
            return;

        var rotation = transform.LocalRotation + (_eyeManager.CurrentEye.Rotation - (transform.LocalRotation - transform.WorldRotation));

        if (rotation.GetDir() is Direction.SouthEast or Direction.East or Direction.NorthEast or Direction.North)
        {
            rotationVisuals.HorizontalRotation = Angle.FromDegrees(270);
            sprite.Rotation = Angle.FromDegrees(270);
            return;
        }

        rotationVisuals.HorizontalRotation = Angle.FromDegrees(90);
        sprite.Rotation = Angle.FromDegrees(90);
    }

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
    }
}
