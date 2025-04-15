using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

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
    public ProjectileIFFColor Color = ProjectileIFFColor.White;
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

[Serializable, NetSerializable]
public enum ProjectileIFFColor : int
{
    White = 0,
    Red = 1,
    Green = 2,
    Blue = 3,
    Pink = 4,
    Magenta = 5,
    Yellow = 6,
}
