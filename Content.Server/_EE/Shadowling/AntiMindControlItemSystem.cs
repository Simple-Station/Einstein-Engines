using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Popups;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the Anti-Mind control item system
/// Its the same as enthralling, but takes some seconds more.
/// </summary>
public sealed class AntiMindControlItemSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AntiMindControlItemComponent, AfterInteractEvent>(AfterInteract);
        SubscribeLocalEvent<AntiMindControlItemComponent, AntiMindControlItemDoAfterEvent>(AntiMindControlDoAfter);
    }

    private void AfterInteract(EntityUid uid, AntiMindControlItemComponent component, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        if (args.Target == null)
            return;

        var target = args.Target.Value;

        if (args.User == target)
            return;

        if (!HasComp<HumanoidAppearanceComponent>(target))
            return;

        var doAfter = new DoAfterArgs(
                EntityManager,
                args.User,
                component.Duration,
                new AntiMindControlItemDoAfterEvent(),
                component.Owner,
                target,
                args.Used)
        {
                CancelDuplicate = true,
                BreakOnDamage = true,
                NeedHand = true,
                BreakOnHandChange = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
        args.Handled = true;
    }

    private void AntiMindControlDoAfter(EntityUid uid, AntiMindControlItemComponent component, AntiMindControlItemDoAfterEvent args)
    {
        if (args.Used == null)
            return;

        if (args.Cancelled)
            return;

        if (args.Args.Target == null)
            return;

        var target = args.Args.Target.Value;
        var user = args.Args.User;

        if (HasComp<LesserShadowlingComponent>(target))
        {
            _popupSystem.PopupEntity(Loc.GetString("mind-control-lesser-shadowling"), user, user, PopupType.MediumCaution);
            return;
        }

        if (HasComp<ThrallComponent>(target))
        {
            RemComp<ThrallComponent>(target);

            if (!HasComp<EnthrallResistanceComponent>(target))
                EnsureComp<EnthrallResistanceComponent>(target);

            var enthrallRes = EntityManager.GetComponent<EnthrallResistanceComponent>(target);
            enthrallRes.ExtraTime += enthrallRes.ExtraTimeUpdate;

            _popupSystem.PopupEntity(Loc.GetString("mind-control-thrall-done"), target, target, PopupType.MediumCaution);
        }
    }
}
