/*
* This file is licensed under AGPLv3
* Copyright (c) 2024 Rane
* See AGPLv3.txt for details.
*/

using Content.Shared.SegmentedEntity;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using System.Numerics;


namespace Content.Client.DeltaV.Lamiae;

/// <summary>
/// This draws lamia segments directly from polygons instead of sprites. This is a very novel approach as of the time this is being written (August 2024) but it wouldn't surprise me
/// if there's a better way to do this at some point. Currently we have a very heavy restriction on the tools we can make, forcing me to make several helpers that may be redundant later.
/// This will be overcommented because I know you haven't seen code like this before and you might want to copy it.
/// This is an expansion on some techniques I discovered in (https://github.com/Elijahrane/Delta-v/blob/49d76c437740eab79fc622ab50d628b926e6ddcb/Content.Client/DeltaV/Arcade/S3D/Renderer/S3DRenderer.cs)
/// </summary>
public sealed class SnakeOverlay : Overlay
{

    private readonly IEntityManager _entManager;
    private readonly SharedTransformSystem _transform;

    // Look through these carefully. WorldSpace is useful for debugging. Note that this defaults to "screen space" which breaks when you try and get the world handle.
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    // Overlays are strange and you need this pattern where you define readonly deps above, and then make a constructor with this pattern. Anything that creates this overlay will then
    // have to provide all the deps.
    public SnakeOverlay(IEntityManager entManager)
    {
        // we get ent manager from SnakeOverlaySystem turning this on and passing it
        _entManager = entManager;
        // with ent manager we can fetch our other entity systems
        _transform = _entManager.EntitySysManager.GetEntitySystem<SharedTransformSystem>();
    }

    // This step occurs each frame. For some overlays you may want to conisder limiting how often they update, but for player entities that move around fast we'll just do it every frame.
    protected override void Draw(in OverlayDrawArgs args)
    {
        // load the handle, the "pen" we draw with
        var handle = args.WorldHandle;

        // Get all lamiae the client knows of and their transform in a way we can enumerate over
        var enumerator = _entManager.AllEntityQueryEnumerator<SegmentedEntityComponent, TransformComponent>();

        // I go over the collection above, pulling out an EntityUid and the two components I need for each.
        while (enumerator.MoveNext(out var uid, out var lamia, out var xform))
        {
            // Skip ones that are off-map. "Map" in this context means interconnected stuff you can travel between by moving, rather than needing e.g. FTL to load a new map.
            if (xform.MapID != args.MapId)
                continue;

            // Skip ones where there's nothing to draw or they're maybe uninitialized
            // if (lamia.Segments.Count == 0)
            //     continue;

            // By the way, there's a hack to mitigate overdraw somewhat. Check out whatever is going on with the variable called "bounds" in DoAfterOverlay.
            // I won't do it here because (1) it's ugly and (2) theoretically these entities can be fucking huge and you'll see the tail end of them when they are way off screen.
            // On a PVS level I think segmented entities should be all-or-nothing when it comes to PVS range, that is you either load all of their segments or none.
            DrawLamia(handle, uid, lamia, xform);
        }
    }

    // This is where we do the actual drawing.
    private void DrawLamia(DrawingHandleWorld handle, EntityUid uid, SegmentedEntityComponent lamia, TransformComponent xform)
    {
        // The handle has already been set for us at the world position of the lamia. We can start drawing from there.
        handle.DrawCircle(_transform.GetWorldPosition(xform), 1f, Color.Red);
    }
}