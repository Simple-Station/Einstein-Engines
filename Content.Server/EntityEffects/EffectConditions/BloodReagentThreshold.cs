using Content.Server.Body.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Chemistry.EntitySystems;

namespace Content.Server.EntityEffects.EffectConditions;

public sealed partial class BloodReagentThreshold : EntityEffectCondition
{
    [DataField]
    public FixedPoint2 Min = FixedPoint2.Zero;

    [DataField]
    public FixedPoint2 Max = FixedPoint2.MaxValue;

    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<ReagentPrototype>))]
    public string? Reagent = null;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (args == null || args.EntityManager == null)
        {
            return false;
        }

        if (Reagent is null)
        {
            return true;
        }

        if (args.EntityManager.TryGetComponent<BloodstreamComponent>(args.TargetEntity, out var blood))
        {
            // Try to resolve the chemical solution
            if (!args.EntityManager.System<SharedSolutionContainerSystem>().ResolveSolution(
                    args.TargetEntity,
                    blood.ChemicalSolutionName,
                    ref blood.ChemicalSolution,
                    out var chemSolution))
            {
                // Failed to resolve solution, apply same logic as when reagent isn't found
                return Min <= 0;
            }

            // Solution resolved, check reagent quantity
            var reagentID = new ReagentId(Reagent, null);
            if (!chemSolution.TryGetReagentQuantity(reagentID, out var quant))
            {
                // Reagent not found in solution
                return Min <= 0;
            }

            // Reagent found, check if within thresholds
            return quant > Min && quant < Max;
        }

        return false;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        ReagentPrototype? reagentProto = null;
        if (Reagent is not null)
            prototype.TryIndex(Reagent, out reagentProto);

        return Loc.GetString("reagent-effect-condition-guidebook-blood-reagent-threshold",
            ("reagent", reagentProto?.LocalizedName ?? Loc.GetString("reagent-effect-condition-guidebook-this-reagent")),
            ("max", Max == FixedPoint2.MaxValue ? (float) int.MaxValue : Max.Float()),
            ("min", Min.Float()));
    }
}
