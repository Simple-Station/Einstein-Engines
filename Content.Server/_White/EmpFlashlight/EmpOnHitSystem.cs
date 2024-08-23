using Content.Shared.Weapons.Melee.Events;
using Content.Server.Emp;
using Content.Shared.Charges.Systems;
using Content.Shared.Charges.Components;

namespace Content.Server._White.EmpFlashlight;

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

        if (!TryComp<LimitedChargesComponent>(uid, out LimitedChargesComponent? charges))
            return false;

        if (_charges.IsEmpty(uid, charges))
            return false;

        if (args.HitEntities.Count > 0)
        {
            _charges.UseCharge(uid,charges);
            return true;
        }

        return false;
    }

    private void HandleEmpHit(EntityUid uid, EmpOnHitComponent comp, MeleeHitEvent args)
    {
        if (!TryEmpHit(uid, comp, args))
            return;

        foreach (var affected in args.HitEntities)
        {
            _emp.EmpPulse(_transform.GetMapCoordinates(affected), comp.Range, comp.EnergyConsumption, comp.DisableDuration);
        }

        args.Handled = true;
    }
}

