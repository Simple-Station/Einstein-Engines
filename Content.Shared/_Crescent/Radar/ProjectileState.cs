using Robust.Shared.Maths;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Crescent.Radar;

/// <summary>
/// State of each individual projectile for interface purposes
/// </summary>
[Serializable, NetSerializable]
public sealed class ProjectileState
{
    public NetCoordinates Coordinates;
    public int VisualTypeIndex;
    public Color Color;
    public float Scale = 1f;
}
