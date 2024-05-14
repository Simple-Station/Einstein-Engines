using Content.Shared.Actions;
using Content.Shared.Aliens.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;

namespace Content.Shared.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedAlienStalkSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly SharedPlasmaVesselSystem _plasmaVessel = default!;
    public override void Initialize()
    {
        base. Initialize();

        SubscribeLocalEvent<AlienStalkComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<AlienStalkComponent, AlienStalkActionEvent>(OnStalk);
    }

    private void OnComponentInit(EntityUid uid, AlienStalkComponent component, ComponentInit args)
    {
        _actionsSystem.AddAction(uid, ref component.StalkActionEntity, component.StalkAction, uid);

        component.Sprint = EnsureComp<MovementSpeedModifierComponent>(uid).BaseWalkSpeed;
    }

    private void OnStalk(EntityUid uid, AlienStalkComponent component, AlienStalkActionEvent args)
    {
        var stealth = EnsureComp<StealthComponent>(uid);
        var movementSpeedMofifier = EnsureComp<MovementSpeedModifierComponent>(uid);
        var sprint = component.Sprint;
        component.Sprint = movementSpeedMofifier.BaseSprintSpeed;
        _movementSpeedModifier.ChangeBaseSpeed(uid, movementSpeedMofifier.BaseWalkSpeed, sprint,
            movementSpeedMofifier.Acceleration);
        _stealth.SetVisibility(uid, 0.2f, stealth);
        _stealth.SetEnabled(uid, !component.IsActive, stealth);
        component.IsActive = !component.IsActive;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PlasmaVesselComponent>();
        while (query.MoveNext(out var uid, out var alien))
        {
            if(TryComp<AlienStalkComponent>(uid, out var stalk)
               && alien.Plasma >= stalk.PlasmaCost
                && stalk.IsActive)
            {
                    _plasmaVessel.ChangePlasmaAmount(uid, -stalk.PlasmaCost, alien);
            }
        }
    }
}

public sealed partial class AlienStalkActionEvent : InstantActionEvent { }
