using Robust.Shared.Physics;

namespace Content.Shared.Weapons.Ranged.Events;

/// <summary>
///     Raised after an entity fires a hitscan weapon, but before the list is truncated to the first target. Necessary for Entities that need to prevent friendly fire
/// </summary>
[ByRefEvent]
public struct HitScanAfterRayCastEvent
{
    public List<RayCastResults>? RayCastResults;

    public HitScanAfterRayCastEvent(List<RayCastResults>? rayCastResults)
    {
        RayCastResults = rayCastResults;
    }
}
