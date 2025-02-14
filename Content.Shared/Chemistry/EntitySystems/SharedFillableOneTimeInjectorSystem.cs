using Content.Shared.Administration.Logs;
using Content.Shared.Chemistry.Components;
using Content.Shared.CombatMode;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Player;

namespace Content.Shared.Chemistry.EntitySystems;

public abstract class SharedFillableOneTimeInjectorSystem : EntitySystem
{
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly SharedSolutionContainerSystem SolutionContainers = default!;
    [Dependency] protected readonly MobStateSystem MobState = default!;
    [Dependency] protected readonly SharedCombatModeSystem Combat = default!;
    [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;
    [Dependency] protected readonly ISharedAdminLogManager AdminLogger = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FillableOneTimeInjectorComponent, GetVerbsEvent<AlternativeVerb>>(AddSetTransferVerbs);
        SubscribeLocalEvent<FillableOneTimeInjectorComponent, ComponentStartup>(OnInjectorStartup);
        SubscribeLocalEvent<FillableOneTimeInjectorComponent, UseInHandEvent>(OnInjectorUse);
    }

    private void AddSetTransferVerbs(Entity<FillableOneTimeInjectorComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || entity.Comp.ToggleState != FillableOneTimeInjectorToggleMode.Draw)
            return;

        var user = args.User;
        var (_, component) = entity;

        var min = component.MinimumTransferAmount;
        var max = component.MaximumTransferAmount;
        var cur = component.TransferAmount;

        var toggleAmount = cur == max ? min : max;

        var priority = 0;
        AlternativeVerb toggleVerb = new()
        {
            Text = Loc.GetString("comp-solution-transfer-verb-toggle", ("amount", toggleAmount)),
            Category = VerbCategory.SetTransferAmount,
            Act = () =>
            {
                component.TransferAmount = toggleAmount;
                Popup.PopupClient(Loc.GetString("comp-solution-transfer-set-amount", ("amount", toggleAmount)), user, user);
                Dirty(entity);
            },

            Priority = priority
        };
        args.Verbs.Add(toggleVerb);

        priority -= 1;

        // Add specific transfer verbs according to the container's size
        for (var amount = entity.Comp.MinimumTransferAmount; amount <= entity.Comp.MaximumTransferAmount; amount += 1)
        {
            var transferAmount = amount;
            AlternativeVerb verb = new()
            {
                Text = Loc.GetString("comp-solution-transfer-verb-amount", ("amount", amount)),
                Category = VerbCategory.SetTransferAmount,
                Act = () =>
                {
                    component.TransferAmount = transferAmount;
                    Popup.PopupClient(Loc.GetString("comp-solution-transfer-set-amount", ("amount", transferAmount)), user, user);
                    Dirty(entity);
                },

                // we want to sort by size, not alphabetically by the verb text.
                Priority = priority
            };

            priority -= 1;

            args.Verbs.Add(verb);
        }
    }

    private void OnInjectorUse(Entity<FillableOneTimeInjectorComponent> entity, ref UseInHandEvent args)
    {
        if (args.Handled || entity.Comp.ToggleState != FillableOneTimeInjectorToggleMode.Draw)
            return;

        SetMode(entity, FillableOneTimeInjectorToggleMode.Inject);
        args.Handled = true;
    }

    private void OnInjectorStartup(Entity<FillableOneTimeInjectorComponent> entity, ref ComponentStartup args)
    {
        // ???? why ?????
        Dirty(entity);
    }

    public void SetMode(Entity<FillableOneTimeInjectorComponent> injector, FillableOneTimeInjectorToggleMode mode)
    {
        injector.Comp.ToggleState = mode;

        if (mode == FillableOneTimeInjectorToggleMode.Inject)
            injector.Comp.TransferAmount = injector.Comp.MaximumTransferAmount;

        Dirty(injector);
    }
}
