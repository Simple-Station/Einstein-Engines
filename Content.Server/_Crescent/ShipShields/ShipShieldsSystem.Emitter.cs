using Content.Shared._Crescent.ShipShields;
using Content.Server.Power.Components;
using Content.Shared.Projectiles;
using Robust.Shared.Physics.Components;
using Content.Server.Emp;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Station.Systems;
using Robust.Shared.Audio.Systems;
using Content.Shared.Examine;
using Content.Server.Explosion.Components;

namespace Content.Server._Crescent.ShipShields;
public partial class ShipShieldsSystem
{
    private const float MAX_EMP_DAMAGE = 10000f;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public void InitializeEmitters()
    {
        SubscribeLocalEvent<ShipShieldEmitterComponent, ShieldDeflectedEvent>(OnShieldDeflected);
        SubscribeLocalEvent<ShipShieldEmitterComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<ShipShieldEmitterComponent, ComponentRemove>(OnRemoved);
    }


    private void OnRemoved(Entity<ShipShieldEmitterComponent> owner,ref ComponentRemove remove)
    {
        var parent = Transform(owner.Owner).GridUid;
        if (parent is null)
            return;
        UnshieldEntity(parent.Value, null);
    }

    private void OnShieldDeflected(EntityUid uid, ShipShieldEmitterComponent component, ShieldDeflectedEvent args)
    {
        if (TryComp<EmpOnTriggerComponent>(args.Deflected, out var emp))
        {
            component.Damage += Math.Clamp(emp.EnergyConsumption, 0f, MAX_EMP_DAMAGE);
            _trigger.Trigger(args.Deflected);
        }

        if (TryComp<ExplosiveComponent>(args.Deflected, out var exp))
        {
            component.Damage += exp.TotalIntensity / 15; //after mlg intensity explosion changes, 1 intensity = 1 dmg, instead of 1 intensity = 15 dmg;
        }

        if (TryComp<ProjectileComponent>(args.Deflected, out var proj))
        {
            component.Damage += (float) proj.Damage.GetTotal();
            proj.DamagedEntity = true;
        }
        else if (TryComp<PhysicsComponent>(args.Deflected, out var phys))
        {
            component.Damage += phys.FixturesMass;
        }

        QueueDel(args.Deflected);
    }

    private void OnExamined(EntityUid uid, ShipShieldEmitterComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (component.Damage == 0f)
        {
            args.PushMarkup(Loc.GetString("shield-emitter-examine-undamaged"));
            return;
        }

        var ratio = component.Damage / component.DamageLimit;

        args.PushMarkup(Loc.GetString("shield-emitter-examine-damaged", ("percent", ratio)));
    }

    // .2 - 2025. commented out because shields draw a fixed amount of power now
    // private void AdjustEmitterLoad(EntityUid uid, ShipShieldEmitterComponent? emitter = null, ApcPowerReceiverComponent? receiver = null)
    // {
    //     if (!Resolve(uid, ref emitter, ref receiver))
    //         return;

    //     /// Raise damage to the power of the growth exponent
    //     var additionalLoad = (float) Math.Clamp(Math.Pow(emitter.Damage, emitter.DamageExp), 0f, emitter.MaxDraw);

    //     receiver.Load = emitter.BaseDraw + additionalLoad;
    // }
}
