/*using Content.Shared.Damage;
using Content.Shared.Body.Organ;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.FixedPoint;

// Namespace has set accessors, leaving it on the default.
namespace Content.Shared.Body.Systems;

public partial class SharedBodySystem
{
    /// <summary>
    ///     Essentially we want to check on every tick if the organ is within the acceptable thresholds. And if it isnt
    ///     then we will degrade the organ by dealing damage to it. The further away from the threshold, the more damage.
    /// </summary>

    public void UpdateOrgan(float frameTime)
    {
        var query = EntityQueryEnumerator<OrganComponent, DamageableComponent>();
        var now = _gameTiming.CurTime;
        while (query.MoveNext(out var uid, out var organ, out var damageable))
        {
            if (organ.Body is not { } body || now < organ.NextUpdate || _mobState.IsDead(body))
                continue;

            organ.NextUpdate = now + organ.UpdateDelay;

            if (organ.Status < organ.DamagedStatus)
                _damageable.TryChangeDamage(uid, GetHealingSpecifier(organ), canSever: false);

            if (organ.Status != OrganStatus.Ruined && !organ.Enabled)
            {
                var ev = new OrganEnabledEvent((uid, organ));
                RaiseLocalEvent(uid, ref ev);
            }

            if (organ.Status == OrganStatus.Ruined && organ.Enabled)
            {
                var ev = new OrganDisabledEvent((uid, organ));
                RaiseLocalEvent(uid, ref ev);
            }
        }
    }

    private void OnDamageChanged(EntityUid entity, OrganComponent component, ref DamageChangedEvent args)
    {
        if (!TryComp<DamageableComponent>(entity, out var damageable))
            return;

        OrganStatus newStatus = GetOrganStatus(component, damageable.TotalDamage);
        if (newStatus != component.Status)
            component.Status = newStatus;

        var ev = new OrganDamageChangedEvent(args.DamageIncreased);
        RaiseLocalEvent(entity, ev);
    }

    public DamageSpecifier GetHealingSpecifier(OrganComponent organ)
    {
        var damage = new DamageSpecifier()
        {
            DamageDict = new Dictionary<string, FixedPoint2>()
            {
                { "Decay", -organ.SelfHealingAmount },
                { "Trauma", -organ.SelfHealingAmount },
            }
        };

        return damage;
    }

    /// <summary>
    /// Fetches the OrganStatus equivalent of the current integrity value for the organ.
    /// </summary>
    public static OrganStatus GetOrganStatus(OrganComponent component, FixedPoint2 integrity)
    {
        var targetIntegrity = OrganStatus.Healthy;
        foreach (var threshold in component.IntegrityThresholds)
        {
            if (integrity <= threshold.Value)
                targetIntegrity = threshold.Key;
        }

        return targetIntegrity;
    }



}

*/