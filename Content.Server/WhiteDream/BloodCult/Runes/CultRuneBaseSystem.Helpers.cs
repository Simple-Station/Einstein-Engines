using Content.Shared.Humanoid;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;

namespace Content.Server.WhiteDream.BloodCult.Runes;

public sealed partial class CultRuneBaseSystem
{
    [Dependency] private readonly PullingSystem _pulling = default!;

    /// <summary>
    ///     Gets all the humanoids near rune.
    /// </summary>
    /// <param name="rune">The rune itself.</param>
    /// <param name="range">Radius for a lookup.</param>
    /// <param name="exlude">Filter to exlude from return.</param>
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

    /// <summary>
    ///     Is used to stop target from pulling/being pulled before teleporting them.
    /// </summary>
    public void StopPulling(EntityUid target)
    {
        if (TryComp(target, out PullableComponent? pullable) && pullable.BeingPulled)
        {
            _pulling.TryStopPull(target, pullable);
        }

        // I wish there was a better way to do it
        if (TryComp(target, out PullerComponent? puller) &&
            puller.Pulling is { } pulledEntity &&
            TryComp(pulledEntity, out PullableComponent? pulledEntityComponent))
        {
            _pulling.TryStopPull(pulledEntity, pulledEntityComponent);
        }
    }
}
