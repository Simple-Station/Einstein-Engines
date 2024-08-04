/*
* This file is licensed under AGPLv3
* Copyright (c) 2024 Rane
* See AGPLv3.txt for details.
*/

using Content.Shared.SegmentedEntity;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Client.Resources;
using Robust.Client.ResourceManagement;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using System.Numerics;
using System.Linq;


namespace Content.Client.DeltaV.Lamiae;

/// <summary>
/// This draws lamia segments directly from polygons instead of sprites. This is a very novel approach as of the time this is being written (August 2024) but it wouldn't surprise me
/// if there's a better way to do this at some point. Currently we have a very heavy restriction on the tools we can make, forcing me to make several helpers that may be redundant later.
/// This will be overcommented because I know you haven't seen code like this before and you might want to copy it.
/// This is an expansion on some techniques I discovered in (https://github.com/Elijahrane/Delta-v/blob/49d76c437740eab79fc622ab50d628b926e6ddcb/Content.Client/DeltaV/Arcade/S3D/Renderer/S3DRenderer.cs)
/// </summary>
public sealed class SnakeOverlay : Overlay
{
    private readonly IResourceCache _resourceCache;
    private readonly IEntityManager _entManager;
    private readonly SharedTransformSystem _transform;
    private readonly SharedHumanoidAppearanceSystem _humanoid = default!;

    // Look through these carefully. WorldSpace is useful for debugging. Note that this defaults to "screen space" which breaks when you try and get the world handle.
    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    // Overlays are strange and you need this pattern where you define readonly deps above, and then make a constructor with this pattern. Anything that creates this overlay will then
    // have to provide all the deps.
    public SnakeOverlay(IEntityManager entManager, IResourceCache resourceCache)
    {
        _resourceCache = resourceCache;
        // we get ent manager from SnakeOverlaySystem turning this on and passing it
        _entManager = entManager;
        // with ent manager we can fetch our other entity systems
        _transform = _entManager.EntitySysManager.GetEntitySystem<SharedTransformSystem>();
        _humanoid = _entManager.EntitySysManager.GetEntitySystem<SharedHumanoidAppearanceSystem>();

        // draw at drawdepth 3
        ZIndex = 3;
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
            if (lamia.Segments.Count == 0)
            {
                _entManager.Dirty(uid, lamia); // pls give me an update...
                continue;
            }

            // By the way, there's a hack to mitigate overdraw somewhat. Check out whatever is going on with the variable called "bounds" in DoAfterOverlay.
            // I won't do it here because (1) it's ugly and (2) theoretically these entities can be fucking huge and you'll see the tail end of them when they are way off screen.
            // On a PVS level I think segmented entities should be all-or-nothing when it comes to PVS range, that is you either load all of their segments or none.

            // Color.White is drawing without modifying color. For clothed tails, we should use White. For skin, we should use the color of the marking.
            // TODO: Better way to cache this
            if (_entManager.TryGetComponent<HumanoidAppearanceComponent>(uid, out var humanoid))
            {
                if (humanoid.MarkingSet.TryGetCategory(MarkingCategories.Tail, out var tailMarkings))
                {
                    var col = tailMarkings.First().MarkingColors.First();
                    DrawLamia(handle, lamia, col);
                }
            }
            else
            {
                DrawLamia(handle, lamia, Color.White);
            }
        }
    }

    // This is where we do the actual drawing.
    private void DrawLamia(DrawingHandleWorld handle, SegmentedEntityComponent lamia, Color color)
    {
        List<DrawVertexUV2D> verts = new List<DrawVertexUV2D>();

        // TODO: move to component
        float radius = 0.3f;

        Vector2? lastPtCW = null;
        Vector2? lastPtCCW = null;

        // TODO: move to component
        var tex = _resourceCache.GetTexture("/Textures/Nyanotrasen/Mobs/Species/lamia.rsi/segment.png");

        int i = 1;
        // so, for each segment we connect we need 4 verts...
        while (i < lamia.Segments.Count - 1)
        {
            var origin = _transform.GetWorldPosition(lamia.Segments[i - 1]);
            var destination = _transform.GetWorldPosition(lamia.Segments[i]);

            // get direction between the two points and normalize it
            var connectorVec = destination - origin;
            connectorVec = connectorVec.Normalized();

            //get one rotated 90 degrees clockwise
            var offsetVecCW = new Vector2(connectorVec.Y, 0 - connectorVec.X);

            //and counterclockwise
            var offsetVecCCW = new Vector2(0 - connectorVec.Y, connectorVec.X);

            /// tri 1
            if (lastPtCW == null)
            {
                verts.Add(new DrawVertexUV2D(origin + offsetVecCW * radius, Vector2.Zero));
            }
            else
            {
                verts.Add(new DrawVertexUV2D((Vector2) lastPtCW, Vector2.Zero));
            }

            if (lastPtCCW == null)
            {
                verts.Add(new DrawVertexUV2D(origin + offsetVecCCW * radius, new Vector2(1, 0)));
            }
            else
            {
                verts.Add(new DrawVertexUV2D((Vector2) lastPtCCW, new Vector2(1, 0)));
            }

            verts.Add(new DrawVertexUV2D(destination + offsetVecCW * radius, new Vector2(0, 1)));

            // tri 2
            if (lastPtCCW == null)
            {
                verts.Add(new DrawVertexUV2D(origin + offsetVecCCW * radius, new Vector2(1, 0)));
            }
            else
            {
                verts.Add(new DrawVertexUV2D((Vector2) lastPtCCW, new Vector2(1, 0)));
            }

            lastPtCW = destination + offsetVecCW * radius;
            verts.Add(new DrawVertexUV2D((Vector2) lastPtCW, new Vector2(0, 1)));
            lastPtCCW = destination + offsetVecCCW * radius;
            verts.Add(new DrawVertexUV2D((Vector2) lastPtCCW, new Vector2(1, 1)));

            if (lamia.UseTaperSystem)
            {
                // TODO: move to component
                // also can just be a float instead of a bool can't it?
                radius *= 0.93f;
            }

            i++;
        }

        // draw tail (1 tri)
        if (lastPtCW != null && lastPtCCW != null)
        {
            verts.Add(new DrawVertexUV2D((Vector2) lastPtCW, new Vector2(0, 0)));
            verts.Add(new DrawVertexUV2D((Vector2) lastPtCCW, new Vector2(1, 0)));

            var destination = _transform.GetWorldPosition(lamia.Segments.Last());

            verts.Add(new DrawVertexUV2D(destination, new Vector2(0.5f, 1f)));
        }

        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleList, texture: tex, verts.ToArray().AsSpan(), color);
    }
}