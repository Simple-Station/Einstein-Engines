using Content.Shared.Alert;
using Content.Shared.Aliens.Components;
using Content.Shared.Chat;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using PlasmaVesselComponent = Content.Shared.Aliens.Components.PlasmaVesselComponent;

namespace Content.Shared.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedPlasmaVesselSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    public override void Initialize()
    {

    }

    public bool ChangePlasmaGain(EntityUid uid, float modifier, PlasmaVesselComponent? component = null)
    {
        if (component == null)
        {
            return false;
        }
        component.PlasmaPerSecond *= modifier;

        return true;
    }

    public bool ChangePlasmaAmount(EntityUid uid, FixedPoint2 amount, PlasmaVesselComponent? component = null, bool regenCap = false)
    {
        if (!Resolve(uid, ref component))
            return false;

        component.Plasma += amount;

        if (regenCap)
            FixedPoint2.Min(component.Plasma, component.PlasmaRegenCap);

        var stalk = CompOrNull<AlienStalkComponent>(uid);
        if (stalk != null && stalk.IsActive)
        {
            return true;
        }
        if(amount != component.PlasmaUnmodified && amount != component.WeedModifier)
        {
            _popup.PopupEntity(Loc.GetString("alien-plasma-left", ("value", component.Plasma)), uid, uid);
        }

        _alerts.ShowAlert(uid, AlertType.PlasmaCounter, (short) Math.Clamp(Math.Round(component.Plasma.Float() / 50f), 0, 10));

        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PlasmaVesselComponent>();
        while (query.MoveNext(out var uid, out var alien))
        {
            alien.Accumulator += frameTime;

            if (alien.Accumulator <= 1)
                continue;
            alien.Accumulator -= 1;

            var weed = false;
            foreach (var entity in _lookup.GetEntitiesInRange(Transform(uid).Coordinates, 0.1f))
            {
                if (HasComp<PlasmaGainModifierComponent>(entity))
                {
                    alien.PlasmaPerSecond = alien.WeedModifier;
                    weed = true;
                }
            }

            if (!weed)
                alien.PlasmaPerSecond = alien.PlasmaUnmodified;

            if (alien.Plasma < alien.PlasmaRegenCap)
            {
                ChangePlasmaAmount(uid, alien.PlasmaPerSecond, alien, regenCap: true);
            }
        }
    }
}
