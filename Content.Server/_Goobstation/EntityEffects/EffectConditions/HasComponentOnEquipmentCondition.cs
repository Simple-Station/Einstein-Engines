using Content.Shared.EntityEffects;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.EntityEffects.EffectConditions;

/// <summary>
/// Condition that checks if any components specified in <see cref="Components"/> are present on items in the inventory of the target entity.
/// </summary>
public sealed partial class HasComponentOnEquipmentCondition : EntityEffectCondition
{
    /// <summary>
    /// The registry of components to check for on the target entity's equipment.
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry Components = default!;

    /// <summary>
    /// If true, inverts the result of the condition check.
    /// </summary>
    [DataField]
    public bool Invert = false;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (args == null)
        {
            return Invert;
        }

        if (Components == null || Components.Count == 0)
        {
            return Invert;
        }

        if (!args.EntityManager.TryGetComponent<InventoryComponent>(args.TargetEntity, out var inv) ||
            !args.EntityManager.System<InventorySystem>().TryGetContainerSlotEnumerator(args.TargetEntity, out var containerSlotEnumerator, SlotFlags.WITHOUT_POCKET))
        {
            return Invert;
        }

        while (containerSlotEnumerator.NextItem(out var item))
        {
            foreach (var comp in Components)
            {
                if (args.EntityManager.HasComponent(item, comp.Value.Component.GetType()))
                {
                    return !Invert;
                }
            }
        }

        return Invert;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        // This condition should be only used on reactive things and not on metabolisable chemicals
        // since equipment doesn't affect your internal metabolism. Additionally, components don't have
        // user-friendly, or even user-understandable names.
        return "TODO";
    }
}
