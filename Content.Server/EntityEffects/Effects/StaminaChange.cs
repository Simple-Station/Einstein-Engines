using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes; /// probably don't need all this

namespace Content.Server.EntityEffects.Effects
{
    ///<summary>
    ///    Lets a chem heal/deal stamina damage.
    ///</summary>
    public sealed partial class StaminaChange : EntityEffect
    {
        /// <summary>
        /// Stamina damage to apply every cycle.
        ///Positive is damage, negative is healing, like health changes.
        /// </summary>
        [DataField]
        public float Amount = 1.0f;

        public override void Effect(EntityEffectBaseArgs args)
        {
            _stamina.TakeStaminaDamage(uid, Amount, visual: false);
        }
    }
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-stamina-change", ("chance", Probability),
            ("deltasign", MathF.Sign(Amount)));
}
