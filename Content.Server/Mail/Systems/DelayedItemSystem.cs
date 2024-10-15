using Content.Shared.Damage;
using Content.Shared.Hands;
using Robust.Shared.Containers;

namespace Content.Server.Mail.Systems;

/// <summary>
///     A placeholder for another entity, spawned when taken out of a container, with the placeholder deleted shortly after.
///     Useful for storing instant effect entities, e.g. smoke, in the mail.
/// </summary>
public sealed class DelayedItemSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Components.DelayedItemComponent, DropAttemptEvent>(OnDropAttempt);
        SubscribeLocalEvent<Components.DelayedItemComponent, GotEquippedHandEvent>(OnHandEquipped);
        SubscribeLocalEvent<Components.DelayedItemComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<Components.DelayedItemComponent, EntGotRemovedFromContainerMessage>(OnRemovedFromContainer);
    }

    private void OnRemovedFromContainer(EntityUid uid, Components.DelayedItemComponent component, ContainerModifiedMessage args)
    {
        Spawn(component.Item, Transform(uid).Coordinates);
    }

    private void OnHandEquipped(EntityUid uid, Components.DelayedItemComponent component, EquippedHandEvent args)
    {
        EntityManager.DeleteEntity(uid);
    }

    private void OnDropAttempt(EntityUid uid, Components.DelayedItemComponent component, DropAttemptEvent args)
    {
        EntityManager.DeleteEntity(uid);
    }

    private void OnDamageChanged(EntityUid uid, Components.DelayedItemComponent component, DamageChangedEvent args)
    {
        Spawn(component.Item, Transform(uid).Coordinates);
        EntityManager.DeleteEntity(uid);
    }
}
