#region

using System.Numerics;
using Content.Client.Pointing.Components;
using Content.Shared.Pointing;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Animations;

#endregion


namespace Content.Client.Pointing;


public sealed partial class PointingSystem : SharedPointingSystem
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly AnimationPlayerSystem _animationPlayer = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    public void InitializeVisualizer() =>
        SubscribeLocalEvent<PointingArrowComponent, AnimationCompletedEvent>(OnAnimationCompleted);

    private void OnAnimationCompleted(EntityUid uid, PointingArrowComponent component, AnimationCompletedEvent args)
    {
        if (args.Key == component.AnimationKey)
            _animationPlayer.Stop(uid, component.AnimationKey);
    }

    private void BeginPointAnimation(EntityUid uid, Vector2 startPosition, Vector2 offset, string animationKey)
    {
        if (_animationPlayer.HasRunningAnimation(uid, animationKey))
            return;

        startPosition =
            new Angle(_eyeManager.CurrentEye.Rotation + _transformSystem.GetWorldRotation(uid))
                .RotateVec(startPosition);

        var animation = new Animation
        {
            Length = PointDuration,
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames =
                    {
                        // We pad here to prevent improper looping and tighten the overshoot, just a touch
                        new(startPosition, 0f),
                        new(Vector2.Lerp(startPosition, offset, 0.9f), PointKeyTimeMove),
                        new(offset, PointKeyTimeMove),
                        new(Vector2.Zero, PointKeyTimeMove),
                        new(offset, PointKeyTimeHover),
                        new(Vector2.Zero, PointKeyTimeHover),
                        new(offset, PointKeyTimeHover),
                        new(Vector2.Zero, PointKeyTimeHover),
                        new(offset, PointKeyTimeHover),
                        new(Vector2.Zero, PointKeyTimeHover),
                        new(offset, PointKeyTimeHover),
                        new(Vector2.Zero, PointKeyTimeHover)
                    }
                }
            }
        };

        _animationPlayer.Play(uid, animation, animationKey);
    }
}
