using Content.Shared.Weapons.Melee.Events;
using Content.Server.Emp;
using Content.Shared.Charges.Systems;
using Content.Shared.Charges.Components;

namespace Content.Server.EmpFlashlight;

public sealed class EmpOnHitSystem : EntitySystem
{

    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EmpOnHitComponent, MeleeHitEvent>(HandleEmpHit);
    }

    public bool TryEmpHit(EntityUid uid, EmpOnHitComponent comp, MeleeHitEvent args)
    {

        if (!TryComp(uid, out LimitedChargesComponent? charges)
            || _charges.IsEmpty(uid, charges)
            || args.HitEntities.Count <= 0)
            return false;

        _charges.UseCharge(uid, charges);
        return true;
    }

    private void HandleEmpHit(EntityUid uid, EmpOnHitComponent comp, MeleeHitEvent args)
    {
        if (!TryEmpHit(uid, comp, args))
            return;

        foreach (var affected in args.HitEntities)
            _emp.EmpPulse(_transform.GetMapCoordinates(affected), comp.Range, comp.EnergyConsumption, comp.DisableDuration);

        args.Handled = true;
    }
}

