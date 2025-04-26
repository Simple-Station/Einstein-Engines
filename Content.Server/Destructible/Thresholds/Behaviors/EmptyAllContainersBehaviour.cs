using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server.Destructible.Thresholds.Behaviors;

/// <summary>
///     Drop all items from all containers
/// </summary>
[DataDefinition]
public sealed partial class EmptyAllContainersBehaviour : IThresholdBehavior
{
    public void Execute(EntityUid owner, DestructibleSystem destructibleSystem, EntityUid? cause = null)
    {
        var entityManager = destructibleSystem.EntityManager;
        if (!entityManager.EntitySysManager.TryGetEntitySystem<ContainerSystem>(out var containerSystem)
            || !entityManager.HasComponent<ContainerManagerComponent>(owner))
            return;

        foreach (var container in containerSystem.GetAllContainers(owner))
            destructibleSystem.ContainerSystem.EmptyContainer(container, true, entityManager.GetComponent<TransformComponent>(owner).Coordinates);
    }
}
