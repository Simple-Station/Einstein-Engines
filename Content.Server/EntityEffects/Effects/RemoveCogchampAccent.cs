using Content.Server.Atmos.EntitySystems;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Content.Shared.Speech.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

/// <summary>
///     Removes cogchamp's accent from mob.
/// </summary>
public sealed partial class RemoveCogchampAccent : EntityEffect
{
    public override bool ShouldLog => false;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-remove-cogchamp-accent", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args.EntityManager.HasComponent<RatvarianLanguageComponent>(args.TargetEntity))
            args.EntityManager.RemoveComponent<RatvarianLanguageComponent>(args.TargetEntity);
    }
}

