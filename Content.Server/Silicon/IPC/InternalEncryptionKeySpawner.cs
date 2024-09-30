using Content.Shared.Roles;
using Content.Shared.Radio.Components;
using Content.Shared.Containers;
using Robust.Shared.Containers;

namespace Content.Server.Silicon.IPC;
public sealed partial class InternalEncryptionKeySpawner : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    public void TryInsertEncryptionKey(EntityUid target, StartingGearPrototype startingGear, IEntityManager entityManager)
    {
#pragma warning disable CS8073
        if (target == null // target can be null during race conditions intentionally created by awful tests.
#pragma warning restore CS8073
            || !TryComp<EncryptionKeyHolderComponent>(target, out var keyHolderComp)
            || keyHolderComp is null
            || !startingGear.Equipment.TryGetValue("ears", out var earEquipString)
            || string.IsNullOrEmpty(earEquipString))
            return;

        var earEntity = entityManager.SpawnEntity(earEquipString, entityManager.GetComponent<TransformComponent>(target).Coordinates);
        if (!entityManager.HasComponent<EncryptionKeyHolderComponent>(earEntity)
            || !entityManager.TryGetComponent<ContainerFillComponent>(earEntity, out var fillComp)
            || !fillComp.Containers.TryGetValue(EncryptionKeyHolderComponent.KeyContainerName, out var defaultKeys))
            return;

        _container.CleanContainer(keyHolderComp.KeyContainer);

        foreach (var key in defaultKeys)
            entityManager.SpawnInContainerOrDrop(key, target, keyHolderComp.KeyContainer.ID, out _);

        entityManager.QueueDeleteEntity(earEntity);
    }
}
