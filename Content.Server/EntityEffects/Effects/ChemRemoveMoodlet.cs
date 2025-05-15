using Content.Shared.EntityEffects;
using Content.Shared.Mood;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

/// <summary>
///     Removes a moodlet from an entity if present.
/// </summary>
[UsedImplicitly]
public sealed partial class ChemRemoveMoodlet : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        return Loc.GetString("reagent-effect-guidebook-remove-moodlet",
            ("name", protoMan.Index<MoodEffectPrototype>(MoodPrototype.Id)));
    }

    /// <summary>
    ///     The mood prototype to be removed from the entity.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<MoodEffectPrototype> MoodPrototype = default!;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs _)
            return;

        var entityManager = IoCManager.Resolve<EntityManager>();
        var ev = new MoodRemoveEffectEvent(MoodPrototype);
        entityManager.EventBus.RaiseLocalEvent(args.TargetEntity, ev);
    }
}
