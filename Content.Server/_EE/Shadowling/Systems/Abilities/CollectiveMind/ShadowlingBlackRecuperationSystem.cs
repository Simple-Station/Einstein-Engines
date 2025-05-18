using Content.Server.Actions;
using Content.Server.Administration.Systems;
using Content.Server.DoAfter;
using Content.Server.Humanoid;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the Black Recuperation logic.
/// Black Rec. either turns back a dead Thrall to life, OR turns a living Thrall into a Lesser Shadowling by empowering them
/// </summary>
public sealed class ShadowlingBlackRecuperationSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private  readonly HumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ActionsSystem _actionsSystem = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingBlackRecuperationComponent, BlackRecuperationEvent>(OnBlackRec);
        SubscribeLocalEvent<ShadowlingBlackRecuperationComponent, BlackRecuperationDoAfterEvent>(OnBlackRecDoAfter);
    }

    private void OnBlackRec(EntityUid uid, ShadowlingBlackRecuperationComponent component, BlackRecuperationEvent args)
    {
        var target = args.Target;

        if (!HasComp<ThrallComponent>(target))
            return;

        if (_mobStateSystem.IsAlive(target) && HasComp<LesserShadowlingComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-black-rec-lesser-already"), uid, uid, PopupType.MediumCaution);
            return;
        }

        var doAfter = new DoAfterArgs(
            EntityManager,
            uid,
            component.Duration,
            new BlackRecuperationDoAfterEvent(),
            uid,
            target);

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnBlackRecDoAfter(EntityUid uid, ShadowlingBlackRecuperationComponent component, BlackRecuperationDoAfterEvent args)
    {
        if (args.Cancelled)
            return;
        if (args.Args.Target is null)
            return;

        var target = args.Args.Target.Value;

        if (!_mobStateSystem.IsAlive(target))
        {
            _rejuvenate.PerformRejuvenate(target);
            _popup.PopupEntity(Loc.GetString("shadowling-black-rec-revive-done"), uid, target, PopupType.MediumCaution);

            var effectEnt = Spawn(component.BlackRecuperationEffect, _transformSystem.GetMapCoordinates(target));
            _transformSystem.SetParent(effectEnt, target);

            _audio.PlayPvs(component.BlackRecSound, target, AudioParams.Default.WithVolume(-1f));

            _damageable.TryChangeDamage(uid, component.DamageToDeal);
        }
        else
        {
            if (component.LesserShadowlingAmount >= component.LesserShadowlingMaxLimit)
            {
                _popup.PopupEntity(Loc.GetString("shadowling-black-rec-limit"), uid, uid, PopupType.MediumCaution);
                return;
            }

            var newUid = _polymorph.PolymorphEntity(target, component.LesserShadowlingSpeciesProto);
            if (newUid == null)
                return;

            EnsureComp<LesserShadowlingComponent>(newUid.Value);

            if (TryComp<HumanoidAppearanceComponent>(newUid.Value, out var human))
                _humanoidAppearance.AddMarking(newUid.Value, component.LesserShadowlingEyes, Color.Red, true, true, human);


            var effectEnt = Spawn(component.BlackRecuperationEffect, _transformSystem.GetMapCoordinates(newUid.Value));
            _transformSystem.SetParent(effectEnt, newUid.Value);

            component.LesserShadowlingAmount++;
            _popup.PopupEntity(Loc.GetString("shadowling-black-rec-lesser-done"), uid, newUid.Value, PopupType.MediumCaution);

            _audio.PlayPvs(component.BlackRecSound, newUid.Value, AudioParams.Default.WithVolume(-1f));

            _damageable.TryChangeDamage(uid, component.DamageToDeal);
        }
    }
}
