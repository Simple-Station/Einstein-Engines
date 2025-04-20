using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Stunnable;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.Psionics;
using Content.Shared.Actions.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Content.Shared.Interaction.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Throwing;
using Robust.Shared.Timing;

namespace Content.Shared.Abilities.Psionics;

public sealed class PsionicInvisibilityPowerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PsionicComponent, PsionicInvisibilityPowerActionEvent>(OnPowerUsed);
        SubscribeLocalEvent<RemovePsionicInvisibilityOffPowerActionEvent>(OnPowerOff);
        SubscribeLocalEvent<PsionicInvisibilityUsedComponent, ComponentInit>(OnStart);
        SubscribeLocalEvent<PsionicInvisibilityUsedComponent, ComponentShutdown>(OnEnd);
        SubscribeLocalEvent<PsionicInvisibilityUsedComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<PsionicInvisibilityUsedComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<PsionicInvisibilityUsedComponent, ShotAttemptedEvent>(OnShootAttempt);
        SubscribeLocalEvent<PsionicInvisibilityUsedComponent, ThrowAttemptEvent>(OnThrowAttempt);
    }

    // This entire system is disgusting and doesn't comply with newer psi power standards.
    // But all I'm here for is to fix a bug, so bite me - TCJ.
    private void OnPowerUsed(EntityUid uid, PsionicComponent component, PsionicInvisibilityPowerActionEvent args)
    {
        if (!_psionics.OnAttemptPowerUse(args.Performer, "psionic invisibility", true)
            || HasComp<PsionicInvisibilityUsedComponent>(uid))
            return;

        ToggleInvisibility(args.Performer);
        _actions.TryGetActionData(args.Action, out var actionData);
        if (actionData is { UseDelay: not null })
            _actions.SetCooldown(args.Action, actionData.UseDelay.Value / component.CurrentDampening);

        Timer.Spawn(TimeSpan.FromSeconds(args.PowerTimer * component.CurrentAmplification), () => RemComp<PsionicInvisibilityUsedComponent>(uid));
        _psionics.LogPowerUsed(uid, "psionic invisibility",
            args.MinGlimmer * component.CurrentAmplification / component.CurrentDampening,
            args.MaxGlimmer * component.CurrentAmplification / component.CurrentDampening);
        args.Handled = true;
    }

    private void OnPowerOff(RemovePsionicInvisibilityOffPowerActionEvent args)
    {
        if (!HasComp<PsionicInvisibilityUsedComponent>(args.Performer))
            return;

        ToggleInvisibility(args.Performer);
        args.Handled = true;
    }

    private void OnStart(EntityUid uid, PsionicInvisibilityUsedComponent component, ComponentInit args)
    {
        EnsureComp<PsionicallyInvisibleComponent>(uid);
        var stealth = EnsureComp<StealthComponent>(uid);
        _stealth.SetVisibility(uid, 0.66f, stealth);

        if (_net.IsServer)
            _audio.PlayPvs(component.StartSound, uid);

    }

    private void OnEnd(EntityUid uid, PsionicInvisibilityUsedComponent component, ComponentShutdown args)
    {
        if (Terminating(uid))
            return;

        RemComp<PsionicallyInvisibleComponent>(uid);
        RemComp<StealthComponent>(uid);

        if (_net.IsServer)
            _audio.PlayPvs(component.EndSound, uid);

        DirtyEntity(uid);
    }

    private void OnAttackAttempt(EntityUid uid, PsionicInvisibilityUsedComponent component, AttackAttemptEvent args) =>
        RemComp<PsionicInvisibilityUsedComponent>(uid);

    private void OnShootAttempt(EntityUid uid, PsionicInvisibilityUsedComponent component, ShotAttemptedEvent args) =>
        RemComp<PsionicInvisibilityUsedComponent>(uid);

    private void OnThrowAttempt(EntityUid uid, PsionicInvisibilityUsedComponent component, ThrowAttemptEvent args) =>
        RemComp<PsionicInvisibilityUsedComponent>(uid);

    private void OnDamageChanged(EntityUid uid, PsionicInvisibilityUsedComponent component, DamageChangedEvent args)
    {
        if (!TryComp<PsionicComponent>(uid, out var psionic)
            || !args.DamageIncreased || args.DamageDelta is not null && args.DamageDelta.GetTotal() < component.DamageToStun)
            return;

        ToggleInvisibility(uid);
        _stunSystem.TryParalyze(uid, TimeSpan.FromSeconds(component.StunTime * psionic.CurrentAmplification / psionic.CurrentDampening), false);
    }

    public void ToggleInvisibility(EntityUid uid)
    {
        if (!HasComp<PsionicInvisibilityUsedComponent>(uid))
        {
            EnsureComp<PsionicInvisibilityUsedComponent>(uid);
        }
        else
        {
            RemComp<PsionicInvisibilityUsedComponent>(uid);
        }
    }
}
