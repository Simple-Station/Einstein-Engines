using Content.Shared.InteractionVerbs;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Server.InteractionVerbs.Actions;

public sealed partial class ModifyStatusEffectAction : InteractionVerbAction
{
    [DataField(required: true)]
    public ProtoId<StatusEffectPrototype> Effect;

    /// <summary>
    ///     If true, the action will ensure that the system already has the status effect when removing time,
    ///     or will ensure the entity gets the status effect when adding it.
    /// </summary>
    [DataField]
    public bool EnsureEffect = true;

    /// <summary>
    ///     Amount of time added by this action. Can be negative, but then <see cref="EnsureEffect"/> should be false.
    /// </summary>
    [DataField]
    public TimeSpan TimeAdded = TimeSpan.FromSeconds(1);

    public override bool CanPerform(EntityUid user, EntityUid target, bool beforeDelay, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        var statusEffects = deps.EntMan.System<StatusEffectsSystem>();
        if (!statusEffects.CanApplyEffect(target, Effect))
            return false;

        return !EnsureEffect || TimeAdded >= TimeSpan.Zero || statusEffects.HasStatusEffect(target, Effect);
    }

    public override void Perform(EntityUid user, EntityUid target, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        var statusEffects = deps.EntMan.System<StatusEffectsSystem>();

        if (statusEffects.HasStatusEffect(target, Effect))
            statusEffects.TryAddTime(target, Effect, TimeAdded);
        else if (EnsureEffect)
            statusEffects.TryAddStatusEffect(target, Effect, TimeAdded, true);
    }
}
