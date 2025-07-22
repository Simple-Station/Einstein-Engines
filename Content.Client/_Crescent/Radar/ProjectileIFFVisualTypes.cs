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

    public class Square : ProjectileIFFVisualBase
    {
        private readonly float _scale;

        public Square(float scale)
        {
            _scale = scale;
        }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineLoop;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            var size = 0.5f * _scale;
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
        public SolidSquare(float scale) : base(scale) {}

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.TriangleFan;
    }

    public class Circle : ProjectileIFFVisualBase
    {
        private readonly float _scale;

        public Circle(float scale)
        {
            _scale = scale;
        }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineLoop;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            var radius = 0.6f * _scale;
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
        public SolidCircle(float scale) : base(scale) {}

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.TriangleFan;
    }

    public class Triangle : ProjectileIFFVisualBase
    {
        private readonly float _scale;

        public Triangle(float scale)
        {
            _scale = scale;
        }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineLoop;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            var size = 0.6f * _scale;
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
        public SolidTriangle(float scale) : base(scale) {}

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.TriangleFan;
    }

    public class Diamond : ProjectileIFFVisualBase
    {
        private readonly float _scale;

        public Diamond(float scale)
        {
            _scale = scale;
        }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineLoop;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            var size = 0.6f * _scale;
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
        public SolidDiamond(float scale) : base(scale) {}

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.TriangleFan;
    }

    public sealed class SquareReticle : ProjectileIFFVisualBase
    {
        private readonly float _scale;

        public SquareReticle(float scale)
        {
            _scale = scale;
        }

        public override DrawPrimitiveTopology Topology => DrawPrimitiveTopology.LineList;

        public override Vector2[] GetVertice(Vector2 position, Matrix3x2 matrix)
        {
            var size = 1f * _scale;
            var dash = 0.5f * _scale;

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
