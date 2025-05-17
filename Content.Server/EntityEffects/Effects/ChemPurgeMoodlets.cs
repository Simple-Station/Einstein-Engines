using Content.Server.Mood;
using Content.Shared.EntityEffects;
using Content.Shared.Mood;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

/// <summary>
///     Removes all non-categorized moodlets from an entity(anything not "Static" like hunger & thirst).
/// </summary>
[UsedImplicitly]
public sealed partial class ChemPurgeMoodlets : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("reagent-effect-guidebook-purge-moodlets");

    [DataField]
    public bool RemovePermanentMoodlets;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs _)
            return;

        var entityManager = IoCManager.Resolve<EntityManager>();
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        if (!entityManager.TryGetComponent(args.TargetEntity, out MoodComponent? moodComponent))
            return;

        var moodletList = new List<string>();
        foreach (var moodlet in moodComponent.UncategorisedEffects)
        {
            if (!protoMan.TryIndex(moodlet.Key, out MoodEffectPrototype? moodProto)
                || moodProto.Timeout == 0 && !RemovePermanentMoodlets)
                continue;

            moodletList.Add(moodlet.Key);
        }

        foreach (var moodId in moodletList)
            entityManager.EventBus.RaiseLocalEvent(args.TargetEntity, new MoodRemoveEffectEvent(moodId));
    }
}
