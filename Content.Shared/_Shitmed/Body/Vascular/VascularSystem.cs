/*using Content.Shared.FixedPoint;
using Content.Shared.Body.Systems;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Medical;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
namespace Content.Shared._Shitmed.Body.Vascular;

public sealed class VascularSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VascularComponent, MapInitEvent>(OnMapInit, after: [typeof(SharedBodySystem)]);
        SubscribeLocalEvent<VascularComponent, VascularStrainEvent>(OnStrainChange);
        SubscribeLocalEvent<VascularComponent, DefibrillatorZapSuccessEvent>(OnZapSuccess);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<VascularComponent, BodyComponent>();

        while (query.MoveNext(out var uid, out var component, out var body))
        {
            if (_mobState.IsDead(uid))
                continue;

            var isFailing = false;
            foreach (var strain in component.Strains)
            {
                if (strain.Value.Duration.HasValue && _timing.CurTime > strain.Value.Duration.Value)
                    component.Strains.Remove(strain.Key);
            }

            var capacityUtilization = component.CurrentStrain / component.Capacity;
            if (component.HeartRate > component.HeartRateHighThreshold)
                component.TimeOverHigh += TimeSpan.FromSeconds(frameTime);
            else if (component.HeartRate < component.HeartRateLowThreshold)
                component.TimeUnderLow += TimeSpan.FromSeconds(frameTime);
            else
            {
                component.TimeOverHigh = TimeSpan.Zero;
                component.TimeUnderLow = TimeSpan.Zero;
            }

            if (component.TimeUnderLow > component.FailureTime || component.TimeOverHigh > component.FailureTime)
                isFailing = true;

            if (component.CurrentStrain > component.Capacity)
            {
                if (!_body.TryGetBodyOrganEntityComps<HeartComponent>((uid, body), out var hearts))
                {
                    ChangeHeartRate(uid, component, -component.HeartRate);
                    component.Capacity = 0;
                    continue;
                }

                float newCapacity = 0;
                foreach (var (heartUid, heart, organ) in hearts)
                {
                    _damageable.TryChangeDamage(heartUid,
                        new DamageSpecifier(_proto.Index<DamageTypePrototype>("Decay"),
                        (component.CurrentStrain - heart.CurrentCapacity) / (hearts.Count * 100)));

                    if (organ.Enabled && heart.CurrentCapacity > 0)
                        newCapacity += heart.CurrentCapacity;
                }

                if (newCapacity == 0)
                    isFailing = true;

                component.Capacity = newCapacity;
                var heartRateIncrease = Math.Min(capacityUtilization * 10f, 50f);
                ChangeHeartRate(uid, component, heartRateIncrease);
            }

            // Recovery phase - gradually return to normal heart rate if not in failure
            if (!isFailing)
            {
                var averageHeartRate = (component.HeartRateHighThreshold + component.HeartRateLowThreshold) / 2;
                var recoveryMultiplier = Math.Max(0, 1f - capacityUtilization);
                var recovery = Math.Min(
                    component.HeartRateRecoveryRate * recoveryMultiplier * frameTime,
                    Math.Abs(component.HeartRate - averageHeartRate));
                if (component.HeartRate > averageHeartRate)
                    ChangeHeartRate(uid, component, -recovery);
                else if (component.HeartRate < averageHeartRate)
                    ChangeHeartRate(uid, component, recovery);
            }
        }
    }

    private void OnMapInit(EntityUid uid, VascularComponent component, MapInitEvent args)
    {
        if (!TryComp(uid, out BodyComponent? body)
            || !_body.TryGetBodyOrganEntityComps<HeartComponent>((uid, body), out var hearts))
            return;

        foreach (var (heartUid, heart, organ) in hearts)
            if (organ.Enabled && heart.CurrentCapacity > 0)
                component.Capacity += heart.CurrentCapacity;

        ChangeHeartRate(uid, component, (component.HeartRateHighThreshold + component.HeartRateLowThreshold) / 2);
        Dirty(uid, component);
    }

    private void ChangeHeartRate(EntityUid uid, VascularComponent component, float change)
    {
        var oldRate = component.HeartRate;
        var newRate = Math.Clamp(oldRate + change, 0, 250);
        component.HeartRate = newRate;
    }

    private void OnStrainChange(EntityUid uid, VascularComponent component, ref VascularStrainEvent args)
    {
        if (args.Add)
        {
            TimeSpan? duration = args.Duration.HasValue
                ? _timing.CurTime + args.Duration.Value
                : null;

            component.Strains[args.Key] = new StrainData
            {
                Value = args.Value,
                Duration = duration
            };
        }
        else
            component.Strains.Remove(args.Key);
    }

    private void OnZapSuccess(EntityUid uid, VascularComponent component, ref DefibrillatorZapSuccessEvent args)
    {
        ChangeHeartRate(uid, component, ((component.HeartRateHighThreshold + component.HeartRateLowThreshold) / 2) - component.HeartRate);
        component.TimeOverHigh = TimeSpan.Zero;
        component.TimeUnderLow = TimeSpan.Zero;
    }
}*/