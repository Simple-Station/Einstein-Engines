using Content.Shared.Crescent.Radar;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;

namespace Content.Client.Crescent.Radar;

public sealed partial class ProjectileIFFSystem : SharedProjectileIFFSystem
{
    private static readonly float DefaultScale = 1.0f;

    private static readonly IProjectileIFFVisual DefaultVisual = new ProjectileIFFVisuals.Square(DefaultScale);

    // PROJECTILE IFF VISUAL TYPES
    private static readonly Dictionary<ProjectileIFFVisualType, Func<float, IProjectileIFFVisual>> Visuals = new()
    {
        { ProjectileIFFVisualType.Square, scale => new ProjectileIFFVisuals.Square(scale) },
        { ProjectileIFFVisualType.Circle, scale => new ProjectileIFFVisuals.Circle(scale) },
        { ProjectileIFFVisualType.Triangle, scale => new ProjectileIFFVisuals.Triangle(scale) },
        { ProjectileIFFVisualType.Diamond, scale => new ProjectileIFFVisuals.Diamond(scale) },
        { ProjectileIFFVisualType.SolidSquare, scale => new ProjectileIFFVisuals.SolidSquare(scale) },
        { ProjectileIFFVisualType.SolidCircle, scale => new ProjectileIFFVisuals.SolidCircle(scale) },
        { ProjectileIFFVisualType.SolidTriangle, scale => new ProjectileIFFVisuals.SolidTriangle(scale) },
        { ProjectileIFFVisualType.SolidDiamond, scale => new ProjectileIFFVisuals.SolidDiamond(scale) },
        { ProjectileIFFVisualType.SquareReticle, scale => new ProjectileIFFVisuals.SquareReticle(scale) },
    };

    public IProjectileIFFVisual GetVisual(ProjectileIFFVisualType visualType, float scale = 1.0f)
    {
        if (!Visuals.TryGetValue(visualType, out var constructor))
            return DefaultVisual;

        return constructor(scale);
    }
}
