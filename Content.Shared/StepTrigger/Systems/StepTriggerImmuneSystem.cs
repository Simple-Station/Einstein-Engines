using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.StepTrigger.Components;

namespace Content.Shared.StepTrigger.Systems;

public sealed class StepTriggerImmuneSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PreventableStepTriggerComponent, StepTriggerAttemptEvent>(OnStepTriggerClothingAttempt);
        SubscribeLocalEvent<PreventableStepTriggerComponent, ExaminedEvent>(OnExamined);
    }

    private void OnStepTriggerClothingAttempt(Entity<PreventableStepTriggerComponent> ent, ref StepTriggerAttemptEvent args)
    {
        if (args.Source.Comp.TriggerGroups == null)
            return;

        if (TryComp<ProtectedFromStepTriggersComponent>(args.Tripper, out var protectedFromStepTriggers)
                && !args.Source.Comp.TriggerGroups.IsValid(protectedFromStepTriggers)
            || _inventory.TryGetInventoryEntity<ProtectedFromStepTriggersComponent>(args.Tripper, out var inventoryProtectedFromStepTriggers)
                && !args.Source.Comp.TriggerGroups.IsValid(inventoryProtectedFromStepTriggers.Comp?.Whitelist))
            args.Cancelled = true;
    }

    private void OnExamined(EntityUid uid, PreventableStepTriggerComponent component, ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("clothing-required-step-trigger-examine"));
    }
}
