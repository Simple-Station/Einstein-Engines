using System.Numerics;
using Robust.Client.Graphics;

namespace Content.Client.Crescent.Radar;

public interface IProjectileIFFVisual
{
    DrawPrimitiveTopology Topology { get; }
    Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix);
}

public static class ProjectileIFFVisuals
{
    public abstract class ProjectileIFFVisualBase : IProjectileIFFVisual
    {
        public abstract DrawPrimitiveTopology Topology { get; }
        public abstract Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix);
    }

    [Virtual]
    public class Square : ProjectileIFFVisualBase
    {
        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineLoop;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            const float size = 0.5f;
            return
            [
                Vector2.Transform(position + new Vector2(-size, -size), matrix),
                Vector2.Transform(position + new Vector2(size, -size), matrix),
                Vector2.Transform(position + new Vector2(size, size), matrix),
                Vector2.Transform(position + new Vector2(-size, size), matrix)
            ];
        }
    }
    public sealed class SolidSquare : Square
    {
        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.TriangleFan;
    }

    [Virtual]
    public class Circle : ProjectileIFFVisualBase
    {
        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineLoop;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            const float radius = 0.6f;
            const int segments = 8;
            var verts = new Vector2[segments + 1];
            for (var i = 0; i <= segments; i++)
            {
                var angle = i / (float) segments * MathHelper.TwoPi;
                var pos = new Vector2(MathF.Sin(angle), MathF.Cos(angle));
                verts[i] = position + pos * radius;
            }

            return verts;
        }
    }
    public sealed class SolidCircle : Circle
    {
        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.TriangleFan;
    }

    [Virtual]
    public class Triangle : ProjectileIFFVisualBase
    {
        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineLoop;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            const float size = 0.6f;
            return
            [
                Vector2.Transform(position + new Vector2(-size, -size), matrix),
                Vector2.Transform(position + new Vector2(size, -size), matrix),
                Vector2.Transform(position + new Vector2(0, size), matrix)
            ];
        }
    }
    public sealed class SolidTriangle : Triangle
    {
        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.TriangleFan;
    }

    [Virtual]
    public class Diamond : ProjectileIFFVisualBase
    {
        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineLoop;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            const float size = 0.6f;
            return
            [
                Vector2.Transform(position + new Vector2(0, -size), matrix),
                Vector2.Transform(position + new Vector2(size, 0), matrix),
                Vector2.Transform(position + new Vector2(0, size), matrix),
                Vector2.Transform(position + new Vector2(-size, 0), matrix)
            ];
        }
    }
    public sealed class SolidDiamond : Diamond
    {
        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.TriangleFan;
    }

    public sealed class SquareReticle : ProjectileIFFVisualBase
    {
        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineList;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            const float size = 1f;
            const float dash = 0.5f;
            return
            [
                Vector2.Transform(position + new Vector2(-size, -size + dash), matrix),
                Vector2.Transform(position + new Vector2(-size, -size), matrix),
                Vector2.Transform(position + new Vector2(-size, -size), matrix),
                Vector2.Transform(position + new Vector2(-size + dash, -size), matrix),
                Vector2.Transform(position + new Vector2(size - dash, -size), matrix),
                Vector2.Transform(position + new Vector2(size, -size), matrix),
                Vector2.Transform(position + new Vector2(size, -size), matrix),
                Vector2.Transform(position + new Vector2(size, -size + dash), matrix),
                Vector2.Transform(position + new Vector2(size, size - dash), matrix),
                Vector2.Transform(position + new Vector2(size, size), matrix),
                Vector2.Transform(position + new Vector2(size, size), matrix),
                Vector2.Transform(position + new Vector2(size - dash, size), matrix),
                Vector2.Transform(position + new Vector2(-size + dash, size), matrix),
                Vector2.Transform(position + new Vector2(-size, size), matrix),
                Vector2.Transform(position + new Vector2(-size, size), matrix),
                Vector2.Transform(position + new Vector2(-size, size - dash), matrix),
            ];
        }
    }
}
