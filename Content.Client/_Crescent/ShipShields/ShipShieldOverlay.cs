using Content.Shared._Crescent.ShipShields;
using Robust.Client.ResourceManagement;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using System.Numerics;
using Content.Client.Resources;
using Robust.Client.Physics;
using Robust.Shared.Prototypes;

namespace Content.Client._Crescent.ShipShields;

public sealed class ShipShieldOverlay : Overlay
{
    private readonly IResourceCache _resourceCache;
    private readonly IEntityManager _entManager;
    private readonly FixtureSystem _fixture;
    private readonly SharedPhysicsSystem _physics;
    private readonly ShaderInstance _unshadedShader;
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public ShipShieldOverlay(IEntityManager entityManager, IPrototypeManager prototypeManager, IResourceCache resourceCache)
    {
        _resourceCache = resourceCache;
        _entManager = entityManager;
        _fixture = _entManager.EntitySysManager.GetEntitySystem<FixtureSystem>();
        _physics = _entManager.EntitySysManager.GetEntitySystem<PhysicsSystem>();

        _unshadedShader = prototypeManager.Index<ShaderPrototype>("unshaded").Instance();

        ZIndex = 8;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;

        handle.UseShader(_unshadedShader);

        var enumerator = _entManager.AllEntityQueryEnumerator<ShipShieldVisualsComponent, FixturesComponent, TransformComponent>();
        while (enumerator.MoveNext(out var uid, out var visuals, out var fixtures, out var xform))
        {
            // VANTABLACK OVERDRAW FROM THE DEPTHS OF LAGHELL
            if (xform.MapID != args.MapId)
                continue;

            // TODO: We can probably at least test its parent grid is in PVS range...?

            var fixture = _fixture.GetFixtureOrNull(uid, "shield", fixtures);

            if (fixture == null || fixture.Shape is not ChainShape)
                continue;

            var chain = (ChainShape) fixture.Shape;

            var texture = _resourceCache.GetTexture("/Textures/_Crescent/ShipShields/shieldtex.png");

            DrawShield(handle, uid, chain, xform, texture);
        }
    }

    private void DrawShield(DrawingHandleWorld handle, EntityUid uid, ChainShape chain, TransformComponent xform, Texture tex)
    {
        List<DrawVertexUV2D> verts = new List<DrawVertexUV2D>();

        // The vertices of this fixture are defined relative to local position,
        // so we'll have to add them to this and then use the matrix to put them back in world position.
        var localPos = xform.LocalPosition;

        // If "Transforms" ever get deprecated go ahead and check how DebugPHysicsSystem is drawing chains in this hellworld future
        var transform = _physics.GetPhysicsTransform(uid);

        for (int i = 1; i <= chain.Count; i++)
        {
            // top left corner
            var leftVertex = VertexToWorldPos(chain.Vertices[i - 1], transform);

            // top right corner
            var rightVertex = VertexToWorldPos(chain.Vertices[i], transform);

            // bottom left corner
            var leftCorner = Corner(localPos, leftVertex, transform);

            // bottom right corner
            var rightCorner = Corner(localPos, rightVertex, transform);

            // Assemble 2 triangles.

            // Triangle one: top left, top right, bottom left
            verts.Add(new DrawVertexUV2D(leftVertex, new Vector2(0, 1)));
            verts.Add(new DrawVertexUV2D(rightVertex, new Vector2(1, 1)));
            verts.Add(new DrawVertexUV2D(leftCorner, Vector2.Zero));

            // Triangle two: top right, bottom left, bottom right
            verts.Add(new DrawVertexUV2D(rightVertex, new Vector2(1, 1)));
            verts.Add(new DrawVertexUV2D(leftCorner, Vector2.Zero));
            verts.Add(new DrawVertexUV2D(rightCorner, new Vector2(1, 0)));
        }

        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleList, texture: tex, verts.ToArray().AsSpan(), Color.White);
    }

    private Vector2 VertexToWorldPos(Vector2 vertexPos, Transform transform)
    {
        var vertLocation = Transform.Mul(transform, vertexPos);

        return vertLocation;
    }

    private Vector2 Corner(Vector2 localPos, Vector2 vertexPos, Transform transform, float radius = 1.3f)
    {
        var localXform = Transform.Mul(transform, localPos);
        var cornerPos = Vector2.Subtract(vertexPos, localXform);
        cornerPos.Normalize();
        cornerPos *= radius;

        return Vector2.Subtract(vertexPos, cornerPos);
    }
}
