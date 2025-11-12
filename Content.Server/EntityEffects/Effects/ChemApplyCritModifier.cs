using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization;
using Content.Shared.EntityEffects;
using Content.Shared.Traits.Assorted.Components;
using Content.Shared.Traits.Assorted.Systems;

using Robust.Shared.Log;
using Content.Shared.Popups;

namespace Content.Server.EntityEffects.Effects;

public sealed partial class ChemApplyCritModifier : EntityEffect
{
    [DataField("value")] public float Value = 0f;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager _, IEntitySystemManager __)
    {
        if (Value == 0f)
            return null;

        return Loc.GetString("reagent-effect-guidebook-crit-modifier",
            ("deltasign", Value >= 0f ? 1 : -1),
            ("amount", Math.Abs((int) Value)));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs)
            return;

        var entMan = args.EntityManager;
        var uid = args.TargetEntity;

        var comp = entMan.EnsureComponent<CritModifierComponent>(uid);

        var desired = Value;
        var delta = desired - comp.ChemActive;
        if (delta == 0f)
            return;

        comp.ChemActive = desired;
        entMan.Dirty(uid, comp);

        entMan.EventBus.RaiseLocalEvent(uid, new CritModifierChangedEvent());
    }
}
