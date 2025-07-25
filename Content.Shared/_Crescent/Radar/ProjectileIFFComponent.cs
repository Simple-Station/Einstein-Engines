using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Maths;

namespace Content.Shared.Crescent.Radar;

/// <summary>
/// Handles what a projectile should look like on radar.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ProjectileIFFComponent : Component
{
    [DataField("visualType")]
    public ProjectileIFFVisualType VisualType = ProjectileIFFVisualType.Square;

    [DataField("color")]
    public Color Color = new(255, 255, 255, 255); // White
    
    [DataField("scale")]
    public float Scale = 1.0f; // Editable in YAML to make visuals bigger or smaller
}

[Serializable, NetSerializable]
public enum ProjectileIFFVisualType : int
{
    Square = 0,
    Circle = 1,
    Triangle = 2,
    Diamond = 3,
    SolidSquare = 4,
    SolidCircle = 5,
    SolidTriangle = 6,
    SolidDiamond = 7,
    SquareReticle = 8,
}
