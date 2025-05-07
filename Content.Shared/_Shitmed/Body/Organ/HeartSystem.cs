/*using Content.Shared.Body.Organ;
using Content.Shared.Movement.Systems;
using Content.Shared.Medical;

namespace Content.Shared._Shitmed.Body.Organ;

public sealed class HeartSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifier = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<HeartComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HeartComponent, OrganDamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<HeartAttackComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HeartAttackComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<HeartAttackComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        SubscribeLocalEvent<HeartAttackComponent, DefibrillatorZapSuccessEvent>(OnZapSuccess);
    }

    private void OnStartup(EntityUid uid, HeartComponent component, ComponentStartup args)
    {
        component.CurrentCapacity = component.Capacity;
    }

    private void OnStartup(EntityUid uid, HeartAttackComponent component, ComponentStartup args)
    {
        component.WalkSpeed *= 0.35f;
        component.SprintSpeed *= 0.35f;

        _speedModifier.RefreshMovementSpeedModifiers(uid);
    }

    private void OnShutdown(EntityUid uid, HeartAttackComponent component, ComponentShutdown args)
    {
        component.WalkSpeed /= 0.35f;
        component.SprintSpeed /= 0.35f;

        _speedModifier.RefreshMovementSpeedModifiers(uid);
    }

    private void OnRefreshSpeed(EntityUid uid, HeartAttackComponent component, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(component.WalkSpeed, component.SprintSpeed);
    }

    private void OnZapSuccess(EntityUid uid, HeartAttackComponent component, ref DefibrillatorZapSuccessEvent args)
    {
        RemComp<HeartAttackComponent>(uid);
    }

    private void OnDamageChanged(EntityUid uid, HeartComponent component, ref OrganDamageChangedEvent args)
    {
        if (!TryComp<OrganComponent>(uid, out var organ))
            return;

        component.CurrentCapacity = component.Capacity * (100 / (int) organ.Status);
    }
}*/