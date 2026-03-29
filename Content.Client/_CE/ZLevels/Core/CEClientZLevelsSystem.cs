/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Numerics;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared.Camera;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client._CE.ZLevels.Core;

/// <summary>
/// Only process Eye offset and drawdepth on clientside
/// </summary>
public sealed partial class CEClientZLevelsSystem : CESharedZLevelsSystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;

    public static float ZLevelOffset = 0.7f;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new CEZLevelBlurOverlay());

        SubscribeLocalEvent<CEZPhysicsComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CEZPhysicsComponent, GetEyeOffsetEvent>(OnEyeOffset);
    }

    private void OnEyeOffset(Entity<CEZPhysicsComponent> ent, ref GetEyeOffsetEvent args)
    {
        Angle rotation = _eye.CurrentEye.Rotation * -1;
        var localPosition = GetVisualsLocalPosition((ent, ent), Transform(ent));
        var offset = rotation.RotateVec(new Vector2(0, localPosition * ZLevelOffset));
        args.Offset += offset;
    }

    private void OnStartup(Entity<CEZPhysicsComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (sprite.SnapCardinals)
            return;

        ent.Comp.NoRotDefault = sprite.NoRotation;
        ent.Comp.DrawDepthDefault = sprite.DrawDepth;
        ent.Comp.SpriteOffsetDefault = sprite.Offset;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CEZPhysicsComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var zPhys, out var sprite, out var xform))
        {
            var localPosition = GetVisualsLocalPosition((uid, zPhys), xform);

            sprite.NoRotation = localPosition != 0 || zPhys.NoRotDefault;

            _sprite.SetOffset((uid, sprite), zPhys.SpriteOffsetDefault + new Vector2(0, localPosition * ZLevelOffset));
            _sprite.SetDrawDepth((uid, sprite), localPosition > 0 ? (int)Shared.DrawDepth.DrawDepth.OverMobs : zPhys.DrawDepthDefault);
        }
    }


    public float GetVisualsLocalPosition(Entity<CEZPhysicsComponent?> ent, TransformComponent? xform = null)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return 0;
        if (!Resolve(ent, ref xform, false))
            return 0;

        var pos = ent.Comp.LocalPosition;

        if (xform.ParentUid != xform.MapUid && ZPhyzQuery.TryComp(xform.ParentUid, out var parentZPhys))
            pos = parentZPhys.LocalPosition;

        return pos;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<CEZLevelBlurOverlay>();
    }
}
