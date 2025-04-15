using System.Numerics;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Content.Server.Magic;
using Content.Shared._Crescent.Magic;
using Robust.Shared.Physics.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Components;
using Content.Server.Medical;
using Content.Server.Ghost;
using Content.Shared.Damage;
using Robust.Shared.Player;
using Content.Server.Effects;
using Robust.Shared.GameObjects;

namespace Content.Server._Crescent.Magic;

public sealed class CrescentMagicSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedDoorSystem _doorSystem = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MagicSystem _magic = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly VomitSystem _vomit = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly ColorFlashEffectSystem _color = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LesserHackSpellEvent>(OnLesserHackSpell);
        SubscribeLocalEvent<LesserKnockdownSpellEvent>(OnLesserKnockdownSpell);
        SubscribeLocalEvent<LesserSuppressSpellEvent>(OnLesserSuppressSpell);
        SubscribeLocalEvent<GreaterSuppressSpellEvent>(OnGreaterSuppressSpell);
    }

    #region spells

    public void OnLesserHackSpell(LesserHackSpellEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<DoorComponent>(args.Target, out var doorComp))
            return;

        _audio.PlayPvs(args.Sound, args.Performer, AudioParams.Default.WithVolume(args.Volume));

        if (TryComp<DoorBoltComponent>(args.Target, out var bolts))
            _doorSystem.SetBoltsDown((args.Target, bolts), false);

        if (doorComp.State is not DoorState.Open)
            _doorSystem.StartOpening(args.Target);

        _magic.Speak(args);
        args.Handled = true;
    }

    public void OnLesserKnockdownSpell(LesserKnockdownSpellEvent args)
    {
        if (args.Handled)
            return;

        _audio.PlayPvs(args.Sound, args.Performer, AudioParams.Default.WithVolume(args.Volume));

        foreach (var entity in _lookup.GetEntitiesInRange(args.Target, args.Range))
        {
            if (entity == args.Performer || !TryComp<PhysicsComponent>(entity, out var physics) || Transform(entity).Anchored || TryComp<ObserverRoleComponent>(entity, out var ghost))
                continue;

            Vector2 targetVector = Vector2.Normalize(Transform(entity).Coordinates.Position - args.Target.Position);
            targetVector *= args.KnockdownForce;

            if (TryComp<StaminaComponent>(entity, out var stamina))
                _stamina.TakeStaminaDamage(entity, args.StaminaDamage, stamina);
            _physics.SetLinearVelocity(entity, targetVector);
        }
        args.Handled = true;
        _magic.Speak(args);
    }

    public void OnLesserSuppressSpell(LesserSuppressSpellEvent args)
    {
        if (args.Handled)
            return;

        _audio.PlayPvs(args.Sound, args.Target, AudioParams.Default.WithVolume(args.Volume));

        foreach (var entity in _lookup.GetEntitiesInRange(args.Target, args.Range))
        {
            if (entity == args.Performer)
                continue;

            if (TryComp<BloodstreamComponent>(entity, out var bloodstream))
                _bloodstream.TryModifyBleedAmount(entity, args.BleedStacks, bloodstream);

            if (TryComp<BodyComponent>(entity, out var body))
                _vomit.Vomit(entity);

            _color.RaiseEffect(Color.Red, new List<EntityUid>() { entity }, Filter.Pvs(entity, entityManager: EntityManager));
        }
        _magic.Speak(args);
        args.Handled = true;
    }

    public void OnGreaterSuppressSpell(GreaterSuppressSpellEvent args)
    {
        if (args.Handled)
            return;

        _audio.PlayPvs(args.Sound, args.Performer, AudioParams.Default.WithVolume(args.Volume));

        var transform = Transform(args.Performer);
        var coords = transform.Coordinates;

        if (TryComp<BloodstreamComponent>(args.Performer, out var ownBloodstream))
            _bloodstream.TryModifyBleedAmount(args.Performer, args.SelfDamage, ownBloodstream);

        foreach (var entity in _lookup.GetEntitiesInRange(coords, args.Range))
        {
            if (entity == args.Performer)
                continue;

            if (TryComp<BloodstreamComponent>(entity, out var bloodstream))
                _bloodstream.TryModifyBleedAmount(entity, args.BleedStacks, bloodstream);

            if (TryComp<BodyComponent>(entity, out var body))
            {
                _color.RaiseEffect(Color.Red, new List<EntityUid>() { entity }, Filter.Pvs(entity, entityManager: EntityManager));
                _vomit.Vomit(entity);
            }

            if (TryComp<StaminaComponent>(entity, out var stamina))
                _stamina.TakeStaminaDamage(entity, args.StaminaDamage, stamina);
        }
        _magic.Speak(args);
        args.Handled = true;
    }
    #endregion spells
}
