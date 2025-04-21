using Content.Server.Actions;
using Content.Server.Administration.Systems;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared._EE.Shadowling;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles Rapid Re-Hatch logic. An ability that heals all wounds and status effects.
/// </summary>
public sealed class ShadowlingRapidRehatchSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingRapidRehatchComponent, RapidRehatchEvent>(OnRapidRehatch);
        SubscribeLocalEvent<ShadowlingRapidRehatchComponent, RapidRehatchDoAfterEvent>(OnRapidRehatchDoAfter);
    }

    private void OnRapidRehatch(EntityUid uid, ShadowlingRapidRehatchComponent comp, RapidRehatchEvent args)
    {
        var user = args.Performer;

        if (_mobState.IsCritical(user) || _mobState.IsDead(user)) return;

        comp.ActionRapidRehatchEntity = args.Action;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            uid,
            TimeSpan.FromSeconds(comp.DoAfterTime),
            new RapidRehatchDoAfterEvent(),
            user);

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnRapidRehatchDoAfter(EntityUid uid, ShadowlingRapidRehatchComponent comp, RapidRehatchDoAfterEvent args)
    {
        _popup.PopupEntity(Loc.GetString("shadowling-rapid-rehatch-complete"), uid, uid, PopupType.Medium);
        _rejuvenate.PerformRejuvenate(uid);
        //todo: add visuals here
        //todo: play sound here
        _actions.StartUseDelay(comp.ActionRapidRehatchEntity);
    }
}
