using Content.Shared.Crescent.Radar;

namespace Content.Client.Crescent.Radar;

public sealed partial class ProjectileIFFSystem : SharedProjectileIFFSystem
{
    private static readonly IProjectileIFFVisual DefaultVisual = new ProjectileIFFVisuals.Square();

    // PROJECTILE IFF VISUAL TYPES
    private static readonly IProjectileIFFVisual[] Visuals =
    [
        new ProjectileIFFVisuals.Square(),              // Square
        new ProjectileIFFVisuals.Circle(),              // Circle
        new ProjectileIFFVisuals.Triangle(),            // Triangle
        new ProjectileIFFVisuals.Diamond(),             // Diamond
        new ProjectileIFFVisuals.SolidSquare(),         // SolidSquare
        new ProjectileIFFVisuals.SolidCircle(),         // SolidCircle
        new ProjectileIFFVisuals.SolidTriangle(),       // SolidTriangle
        new ProjectileIFFVisuals.SolidDiamond(),        // SolidDiamond
        new ProjectileIFFVisuals.SquareReticle(),       // SquareReticle
    ];

    // PROJECTILE IFF COLORS
    private static readonly Color[] Colors =
    [
        Color.White,        // White
        Color.Red,          // Red
        Color.LightGreen,   // Green
        Color.LightBlue,    // Blue
        Color.Pink,         // Pink
        Color.Magenta,      // Magenta
        Color.Yellow,       // Yellow
    ];

    public IProjectileIFFVisual GetVisual(int visualTypeIndex)
    {
        if (visualTypeIndex < 0 || visualTypeIndex >= Visuals.Length)
        {
            return DefaultVisual;
        }
        return Visuals[visualTypeIndex];
    }

    public Color GetColor(int colorIndex)
    {
        if (colorIndex < 0 || colorIndex >= Colors.Length)
        {
            return Color.White;
        }
        return Colors[colorIndex];
    }
}
