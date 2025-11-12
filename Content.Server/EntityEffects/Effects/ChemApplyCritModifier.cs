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

    protected override string? ReagentEffectGuidebookText(IPrototypeManager proto, IEntitySystemManager entSys)
        => "Adjusts critical threshold while the reagent condition is met.";


    public override void Effect(EntityEffectBaseArgs args)
    {
        Logger.WarningS("critmod", $"ChemApplyCritModifier ran: Value={Value}");
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
