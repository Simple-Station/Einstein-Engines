using Content.Shared.InteractionVerbs;
using Robust.Shared.Random;

namespace Content.Server.InteractionVerbs.Actions;

/// <summary>
///     An action that does nothing on its own, made just to mimic the old "chance to show a popup" interactions.
/// </summary>
public partial class NoOpAction : InteractionVerbAction
{
    [DataField]
    public float SuccessChance = 1f;

    public override bool CanPerform(EntityUid user, EntityUid target, bool isBefore, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        if (isBefore)
            return true; // so the do-after can happen if there's one

        // Return true if chance >= 1f, false if <= 0f, or a random result if anywhere in-between.
        return SuccessChance >= 1f || (SuccessChance >= 0f && deps.Random.Prob(SuccessChance));
    }

    public override void Perform(EntityUid user, EntityUid target, InteractionVerbPrototype proto, VerbDependencies deps)
    {
    }
}
