using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Tesla.Components;
using Content.Server.Lightning;

namespace Content.Server.Tesla.EntitySystems;

/// <summary>
/// Generates electricity from lightning bolts
/// </summary>
public sealed class TeslaCoilSystem : EntitySystem
{
    [Dependency] private readonly BatterySystem _battery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TeslaCoilComponent, LightningEffectEvent>(OnLightningEffect);
    }

    //When struck by lightning, charge the internal battery
    private void OnLightningEffect(Entity<TeslaCoilComponent> coil, ref LightningEffectEvent args)
    {
        if (TryComp<BatteryComponent>(coil, out var batteryComponent))
        {
            _battery.SetCharge(coil, batteryComponent.CurrentCharge + args.Discharge * coil.Comp.LightningChargeEfficiency);
        }
    }
}
