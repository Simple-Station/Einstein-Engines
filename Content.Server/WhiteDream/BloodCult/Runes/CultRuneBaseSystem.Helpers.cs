using Content.Shared.Humanoid;

namespace Content.Server.WhiteDream.BloodCult.Runes;

public sealed partial class CultRuneBaseSystem
{
    public HashSet<Entity<HumanoidAppearanceComponent>> GetTargetsNearRune(EntityUid rune, float range,
        Predicate<Entity<HumanoidAppearanceComponent>>? exlude = null)
    {
        var runeTransform = Transform(rune);
        var possibleTargets = _lookup.GetEntitiesInRange<HumanoidAppearanceComponent>(runeTransform.Coordinates, range);
        if (exlude != null)
        {
            possibleTargets.RemoveWhere(exlude);
        }

        return possibleTargets;
    }
}
