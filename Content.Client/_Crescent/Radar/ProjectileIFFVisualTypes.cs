using System.Numerics;
using Robust.Client.Graphics;

namespace Content.Client.Crescent.Radar;

public interface IProjectileIFFVisual
{
    float Scale { get; set; }

    DrawPrimitiveTopology Topology { get; }
    Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix);
}

public static class ProjectileIFFVisuals
{
    public abstract class ProjectileIFFVisualBase : IProjectileIFFVisual
    {
        public float Scale { get; set; }

        protected ProjectileIFFVisualBase(float scale)
        {
            Scale = scale;
        }

        public abstract DrawPrimitiveTopology Topology { get; }
        public abstract Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix);
    }

    public class Square : ProjectileIFFVisualBase
    {
        public Square(float scale) : base(scale) { }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineLoop;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            var size = 0.5f * Scale;
            return new[]
            {
                Vector2.Transform(position + new Vector2(-size, -size), matrix),
                Vector2.Transform(position + new Vector2(size, -size), matrix),
                Vector2.Transform(position + new Vector2(size, size), matrix),
                Vector2.Transform(position + new Vector2(-size, size), matrix)
            };
        }
    }

    public sealed class SolidSquare : Square
    {
        public SolidSquare(float scale) : base(scale) { }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.TriangleFan;
    }

    public class Circle : ProjectileIFFVisualBase
    {
        public Circle(float scale) : base(scale) { }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineLoop;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            var radius = 0.6f * Scale;
            const int segments = 8;
            var verts = new Vector2[segments + 1];
            for (var i = 0; i <= segments; i++)
            {
                var angle = i / (float)segments * MathHelper.TwoPi;
                var pos = new Vector2(MathF.Sin(angle), MathF.Cos(angle));
                verts[i] = Vector2.Transform(position + pos * radius, matrix);
            }

            return verts;
        }
    }

    public sealed class SolidCircle : Circle
    {
        public SolidCircle(float scale) : base(scale) { }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.TriangleFan;
    }

    public class Triangle : ProjectileIFFVisualBase
    {
        public Triangle(float scale) : base(scale) { }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineLoop;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            var size = 0.6f * Scale;
            return new[]
            {
                Vector2.Transform(position + new Vector2(-size, -size), matrix),
                Vector2.Transform(position + new Vector2(size, -size), matrix),
                Vector2.Transform(position + new Vector2(0, size), matrix)
            };
        }
    }

    public sealed class SolidTriangle : Triangle
    {
        public SolidTriangle(float scale) : base(scale) { }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.TriangleFan;
    }

    public class Diamond : ProjectileIFFVisualBase
    {
        public Diamond(float scale) : base(scale) { }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineLoop;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            var size = 0.6f * Scale;
            return new[]
            {
                Vector2.Transform(position + new Vector2(0, -size), matrix),
                Vector2.Transform(position + new Vector2(size, 0), matrix),
                Vector2.Transform(position + new Vector2(0, size), matrix),
                Vector2.Transform(position + new Vector2(-size, 0), matrix)
            };
        }
    }

    public sealed class SolidDiamond : Diamond
    {
        public SolidDiamond(float scale) : base(scale) { }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.TriangleFan;
    }

    public sealed class SquareReticle : ProjectileIFFVisualBase
    {
        public SquareReticle(float scale) : base(scale) { }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineList;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            var size = 1f * Scale;
            var dash = 0.5f * Scale;

            return new[]
            {
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
            };
        }
    }
}
