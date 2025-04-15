using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Crescent.Radar;

/// <summary>
/// State of each individual turret for interface purposes
/// </summary>
[Serializable, NetSerializable]
public sealed class TurretState
{
    public bool IsControlled;
    public NetCoordinates Coordinates;
}
