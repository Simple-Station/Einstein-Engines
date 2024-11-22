using Content.Client.Rotation;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Rotation;
using Robust.Client.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Client.Buckle;

internal sealed class BuckleSystem : SharedBuckleSystem
{
    [Dependency] private readonly RotationVisualizerSystem _rotationVisualizerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BuckleComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<StrapComponent, MoveEvent>(OnStrapMoveEvent);
        SubscribeLocalEvent<BuckleComponent, BuckledEvent>(OnBuckledEvent);
        SubscribeLocalEvent<BuckleComponent, UnbuckledEvent>(OnUnbuckledEvent);
    }

     /// <summary>
    /// Is the strap entity already rotated north? Lower the draw depth of the buckled entity.
    /// </summary>
    private void OnBuckledEvent(Entity<BuckleComponent> ent, ref BuckledEvent args)
    {
        if (!TryComp<SpriteComponent>(args.Strap, out var strapSprite) ||
            !TryComp<SpriteComponent>(ent.Owner, out var buckledSprite))
            return;

        if (Transform(args.Strap.Owner).LocalRotation.GetCardinalDir() == Direction.North)
        {
            ent.Comp.OriginalDrawDepth ??= buckledSprite.DrawDepth;
            buckledSprite.DrawDepth = strapSprite.DrawDepth - 1;
        }
    }

    /// <summary>
    /// Was the draw depth of the buckled entity lowered? Reset it upon unbuckling.
    /// </summary>
    private void OnUnbuckledEvent(Entity<BuckleComponent> ent, ref UnbuckledEvent args)
    {
        if (!TryComp<SpriteComponent>(ent.Owner, out var buckledSprite))
            return;

        if (ent.Comp.OriginalDrawDepth.HasValue)
        {
            buckledSprite.DrawDepth = ent.Comp.OriginalDrawDepth.Value;
            ent.Comp.OriginalDrawDepth = null;
        }
    }

    private void OnStrapMoveEvent(EntityUid uid, StrapComponent component, ref MoveEvent args)
    {
        // I'm moving this to the client-side system, but for the sake of posterity let's keep this comment:
        // > This is mega cursed. Please somebody save me from Mr Buckle's wild ride

        // The nice thing is its still true, this is quite cursed, though maybe not omega cursed anymore.
        // This code is garbage, it doesn't work with rotated viewports. I need to finally get around to reworking
        // sprite rendering for entity layers & direction dependent sorting.

        if (args.NewRotation == args.OldRotation)
            return;

        if (!TryComp<SpriteComponent>(uid, out var strapSprite))
            return;

        var isNorth = Transform(uid).LocalRotation.GetCardinalDir() == Direction.North;
        foreach (var buckledEntity in component.BuckledEntities)
        {
            if (!TryComp<BuckleComponent>(buckledEntity, out var buckle))
                continue;

            if (!TryComp<SpriteComponent>(buckledEntity, out var buckledSprite))
                continue;

            if (isNorth)
            {
                buckle.OriginalDrawDepth ??= buckledSprite.DrawDepth;
                buckledSprite.DrawDepth = strapSprite.DrawDepth - 1;
            }
            else if (buckle.OriginalDrawDepth.HasValue)
            {
                buckledSprite.DrawDepth = buckle.OriginalDrawDepth.Value;
                buckle.OriginalDrawDepth = null;
            }
        }
    }

    private void OnAppearanceChange(EntityUid uid, BuckleComponent component, ref AppearanceChangeEvent args)
    {
        if (!TryComp<RotationVisualsComponent>(uid, out var rotVisuals)
            || !Appearance.TryGetData<bool>(uid, BuckleVisuals.Buckled, out var buckled, args.Component)
            || !buckled || args.Sprite == null)
            return;

        // Animate strapping yourself to something at a given angle
        // TODO: Dump this when buckle is better
        _rotationVisualizerSystem.AnimateSpriteRotation(uid, args.Sprite, rotVisuals.HorizontalRotation, 0.125f);
    }
}
