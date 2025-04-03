using Content.Shared.Damage.Systems;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

///<summary>
///    Lets a chem heal/deal stamina damage.
///</summary>
public sealed partial class StaminaChange : EntityEffect
{
    [Dependency] private readonly StaminaSystem _stamina = default!;
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-stamina-change", ("chance", Probability), ("deltasign", MathF.Sign(Amount)), ("amount", MathF.Abs(Amount)));

    ///<summary>
    ///     Stamina damage to apply every cycle.
    ///     Positive is damage, negative is healing, like health changes.
    ///</summary>
    [DataField("amount")]
    public float Amount = 1.0f;

    public override void Effect(EntityEffectBaseArgs args)
    {
        args.EntityManager.System<StaminaSystem>().TakeStaminaDamage(args.TargetEntity, Amount, visual: false);
    }
}
