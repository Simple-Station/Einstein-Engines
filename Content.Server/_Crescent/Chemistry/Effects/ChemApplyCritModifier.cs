namespace Content.Server._Crescent.Chemistry.Effects
{
    [RegisterComponent]
    public sealed partial class ChemApplyCritModifier : EntityEffect
    {
        [DataField("threshold")] public float Threshold = 5f;
        [DataField("additive")] public bool Additive = false;
        [DataField("value")] public float Value = 25;
        [DataField("removeOnEnd")] public bool RemoveOnEnd = false;

        public override void Effect(EntityEffectBaseArgs args)
        {
            if (args is not EntityEffectReagentArgs rArgs)
                return;

            var entMan = args.EntityManager;
            var uid = args.TargetEntity;

            var amount = rArgs.Quantity;

            var hasComp = entMan.TryGetComponent(uid, out CritModifierComponent? comp);
            if (amount >= Threshold)
            {
                comp ??= entMan.EnsureComponent<CritModifierComponent>(uid);

                if (comp.OriginalCritThreshold <= 0f && comp.CritThresholdModifier == 0f)
                {
                    comp.OriginalCritThreshold = comp.OriginalCritThreshold;
                }

                comp.CritThresholdModifier = Additive ? comp.CritThresholdModifier + Value : Value;
                entMan.Dirty(uid, comp);
                entMan.RaiseLocalEvent(uid, new CritModifierChangedEvent());
            }
            else
            {
                if (!hasComp)
                    return;

                if (RemoveOnEnd)
                {
                    entMan.RemoveComponent<CritModifierComponent>(uid);
                }
                else
                {
                    comp!.CritThresholdModifier = 0f;
                    entMan.Dirty(uid, comp);
                }
                entMan.RaiseLocalEvent(uid, new CritModifierChangedEvent());
            }
        }
    }
