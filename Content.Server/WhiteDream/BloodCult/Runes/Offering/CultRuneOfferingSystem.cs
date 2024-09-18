using System.Linq;
using Content.Server.Antag;
using Content.Server.Bible.Components;
using Content.Server.Body.Systems;
using Content.Server.Cuffs;
using Content.Server.Mind;
using Content.Server.Stunnable;
using Content.Server.WhiteDream.BloodCult.Gamerule;
using Content.Server.WhiteDream.BloodCult.Runes.Revive;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.Mind.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffect;
using Content.Shared.WhiteDream.BloodCult.Components;
using Robust.Shared.Player;

namespace Content.Server.WhiteDream.BloodCult.Runes.Offering;

public sealed class CultRuneOfferingSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antagSelection = default!;
    [Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly CuffableSystem _cuffable = default!;
    [Dependency] private readonly CultRuneBaseSystem _cultRune = default!;
    [Dependency] private readonly CultRuneReviveSystem _cultRuneRevive = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly StunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CultRuneOfferingComponent, TryInvokeCultRuneEvent>(OnInvoked);
    }

    private void OnInvoked(Entity<CultRuneOfferingComponent> ent, ref TryInvokeCultRuneEvent args)
    {
        var possibleTargets = _cultRune.GetTargetsNearRune(ent, ent.Comp.OfferingRange,
            entity => HasComp<BloodCultistComponent>(entity));

        if (possibleTargets.Count == 0)
            return;

        var victim = possibleTargets.First();
        if (!TryComp(victim, out MobStateComponent? state))
            return;

        _cultRuneRevive.AddCharges(ent, ent.Comp.ReviveChargesPerSacrifice);

        if (!TryComp(victim, out MindContainerComponent? mind)
            || mind.Mind is null
            || _bloodCultRule.IsTarget(mind.Mind.Value)
            || state.CurrentState == MobState.Dead
            || HasComp<BibleUserComponent>(victim)
            || HasComp<MindShieldComponent>(victim))
        {
            Sacrifice(victim);
        }
        else
        {
            Convert(ent, victim, args.User);
        }
    }

    private void Sacrifice(EntityUid target)
    {
        _body.GibBody(target);
        if (!TryComp<MindContainerComponent>(target, out var mindComponent))
        {
            return;
        }

        var transform = CompOrNull<TransformComponent>(target)?.Coordinates;

        if (transform == null)
            return;

        if (!mindComponent.Mind.HasValue)
            return;

        var shard = Spawn("SoulShard", transform.Value);

        _mind.TransferTo(mindComponent.Mind.Value, shard);
        _mind.UnVisit(mindComponent.Mind.Value);
    }

    private void Convert(Entity<CultRuneOfferingComponent> rune, EntityUid target, EntityUid user)
    {
        if (!TryComp(target, out ActorComponent? actor))
        {
            return;
        }

        // I guess that's not the best way to do it but whatever
        _antagSelection.ForceMakeAntag<BloodCultRuleComponent>(actor.PlayerSession, "BloodCult");
        _stun.TryStun(target, TimeSpan.FromSeconds(2f), false);
        if (TryComp(target, out CuffableComponent? cuffs) && cuffs.Container.ContainedEntities.Count >= 1)
        {
            var lastAddedCuffs = cuffs.LastAddedCuffs;
            _cuffable.Uncuff(target, user, lastAddedCuffs);
        }

        _statusEffects.TryRemoveStatusEffect(target, "Muted");
        _damageable.TryChangeDamage(target, rune.Comp.ConvertHealing);

        // RemCompDeferred<BlightComponent>(target); // TODO: Blight component whatever it is
    }
}
