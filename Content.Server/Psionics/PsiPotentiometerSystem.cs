using Content.Server.DoAfter;
using Content.Server.PowerCell;
using Content.Shared.Abilities.Psionics;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.PowerCell;
using Content.Shared.Psionics;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Content.Shared.Actions.Events;

namespace Content.Server.Psionics;

/// <summary>
///     This stores the eye exam system for <see cref="PsiPotentiometerComponent"/>
/// </summary>
public sealed class PsiPotentiometerSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PsiPotentiometerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<PsiPotentiometerComponent, PsiPotentiometerDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<PotentiometryPowerActionEvent>(OnPowerUsed);
    }

    private void OnPowerUsed(PotentiometryPowerActionEvent args)
    {
        if (!TryComp<PsiPotentiometerComponent>(args.Performer, out var comp))
            return;

        args.Handled = TryStartPotentiometry(args.Performer, args.Target, comp);
    }

    private void OnAfterInteract(EntityUid uid, PsiPotentiometerComponent component, AfterInteractEvent args)
    {
        if (args.Handled
            || args.Target is not { } target
            || component.Innate)
            return;

        args.Handled = TryStartPotentiometry(uid, target, args.User, component);
    }

    private void OnDoAfter(Entity<PsiPotentiometerComponent> uid, ref PsiPotentiometerDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled
            || args.Target == null
            || HasComp<PowerCellDrawComponent>(uid) && !_powerCell.HasDrawCharge(uid, user: args.User))
            return;

        OpenUserInterface(args.User, uid);
        GetPotentiometryResults(uid, args.Target.Value);
        args.Handled = true;
    }


    /// <summary>
    ///     Actually handles the exam interaction.
    /// </summary>
    public bool TryStartPotentiometry(EntityUid uid, EntityUid target, EntityUid user, PsiPotentiometerComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (!_uiSystem.HasUi(uid, PsiPotentiometerUiKey.Key))
            EnsureUserInterface(uid, component);

        return _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, component.ExamSpeed, new PsiPotentiometerDoAfterEvent(),
            uid, target, uid)
        {
            BlockDuplicate = true,
            BreakOnUserMove = true,
            BreakOnTargetMove = true,
            BreakOnHandChange = true,
            NeedHand = true
        });
    }

    public bool TryStartPotentiometry(EntityUid user, EntityUid target, PsiPotentiometerComponent component)
    {
        if (!_uiSystem.HasUi(user, PsiPotentiometerUiKey.Key))
            EnsureUserInterface(user, component);

        return _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, component.ExamSpeed, new PsiPotentiometerDoAfterEvent(),
            user, target, user)
        {
            BlockDuplicate = true,
            BreakOnUserMove = true,
            BreakOnTargetMove = true,
            BreakOnHandChange = true,
            NeedHand = true
        });
    }

    private void OpenUserInterface(EntityUid user, EntityUid psiPotentiometer)
    {
        if (!TryComp<ActorComponent>(user, out var actor)
            || !_uiSystem.TryGetUi(psiPotentiometer, PsiPotentiometerUiKey.Key, out var ui))
            return;

        _uiSystem.OpenUi(ui, actor.PlayerSession);
    }

    private void EnsureUserInterface(EntityUid uid, PsiPotentiometerComponent component)
    {
        if (component.PotentiometerUi is null)
            return;

        _uiSystem.AddUi(uid, component.PotentiometerUi);
    }

    /// <summary>
    ///     Runs the checks for metapsionic feedback messages
    /// </summary>
    private void GetPotentiometryResults(EntityUid performer, EntityUid target)
    {
        if (!_uiSystem.TryGetUi(performer, PsiPotentiometerUiKey.Key, out var ui))
            return;

        var feedbackMessages = new List<string>();
        if (HasComp<MindbrokenComponent>(target))
            feedbackMessages.Add("metapsionic-mindbreaking-feedback");

        // Yes, Else If. If the target is mindbroken, they should never be psionic at all. That is reflected here.
        else if (TryComp<PsionicComponent>(target, out var psionic))
        {
            if (target == performer)
                feedbackMessages.Add("metapsionic-self-feedback");

            foreach (var power in psionic.ActivePowers)
                if (power.MetapsionicFeedback is not null)
                    feedbackMessages.Add(power.MetapsionicFeedback);
        }

        var ev = new OnPotentiometryEvent(performer, feedbackMessages);

        if (ev.Messages.Count == 0)
            ev.Messages.Add("metapsionic-no-feedback");

        _uiSystem.SendUiMessage(ui, new PsiPotentiometerUserMessage(GetNetEntity(target), ev.Messages));
    }
}