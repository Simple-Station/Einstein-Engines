using Content.Shared.Crescent.Radar;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;

namespace Content.Client.Crescent.Radar;

public sealed partial class ProjectileIFFSystem : SharedProjectileIFFSystem
{
    private static readonly float scale = 1.0f;

    // PROJECTILE IFF VISUAL TYPES
    private static readonly Dictionary<ProjectileIFFVisualType, IProjectileIFFVisual> VisualInstances = new()
    {
        { ProjectileIFFVisualType.Square, new ProjectileIFFVisuals.Square(scale) },
        { ProjectileIFFVisualType.Circle, new ProjectileIFFVisuals.Circle(scale) },
        { ProjectileIFFVisualType.Triangle, new ProjectileIFFVisuals.Triangle(scale) },
        { ProjectileIFFVisualType.Diamond, new ProjectileIFFVisuals.Diamond(scale) },
        { ProjectileIFFVisualType.SolidSquare, new ProjectileIFFVisuals.SolidSquare(scale) },
        { ProjectileIFFVisualType.SolidCircle, new ProjectileIFFVisuals.SolidCircle(scale) },
        { ProjectileIFFVisualType.SolidTriangle, new ProjectileIFFVisuals.SolidTriangle(scale) },
        { ProjectileIFFVisualType.SolidDiamond, new ProjectileIFFVisuals.SolidDiamond(scale) },
        { ProjectileIFFVisualType.SquareReticle, new ProjectileIFFVisuals.SquareReticle(scale) },
    };

    private static readonly IProjectileIFFVisual DefaultVisual = VisualInstances[ProjectileIFFVisualType.Square];

    public IProjectileIFFVisual GetVisual(ProjectileIFFVisualType visualType, float scale = 1.0f)
    {
        if (!VisualInstances.TryGetValue(visualType, out var visual))
            visual = DefaultVisual;

        // Update the scale of the existing instance before returning it
        visual.Scale = scale;

        return visual;
    }
}
