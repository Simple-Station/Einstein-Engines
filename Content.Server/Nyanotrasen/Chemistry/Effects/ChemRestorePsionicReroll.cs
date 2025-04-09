using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.ReagentEffects
{

    [UsedImplicitly]
    public sealed partial class ChemRestorePsionicReroll : EntityEffect
    {
        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-chem-restorereroll-psionic");

        public override void Effect(EntityEffectBaseArgs args)
        {
            if (args is not EntityEffectReagentArgs _)
                return;

            var psySys = args.EntityManager.EntitySysManager.GetEntitySystem<PsionicsSystem>();
            psySys.RestorePsionicReroll(args.TargetEntity);
        }
    }
}
