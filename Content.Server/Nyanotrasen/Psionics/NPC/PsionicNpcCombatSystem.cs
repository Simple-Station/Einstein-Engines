using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Server.NPC.Events;
using Content.Server.NPC.Components;
using Content.Server.Abilities.Psionics;
using Content.Shared.Psionics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Psionics.NPC;

// TODO this is nyanotrasen shitcode. It works, but it needs to be refactored to be more generic.
public sealed class PsionicNpcCombatSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    private static readonly ProtoId<PsionicPowerPrototype> NoosphericZapProto = "NoosphericZapPower";
    private PsionicPowerPrototype NoosphericZap = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NoosphericZapPowerComponent, NPCSteeringEvent>(ZapCombat);

        NoosphericZap = _protoMan.Index(NoosphericZapProto);
    }

    private void ZapCombat(Entity<NoosphericZapPowerComponent> ent, ref NPCSteeringEvent args)
    {
        // Nothing uses this anyway, what the hell it's pure shitcode?
        // PsionicComponent? psionics = null;
        // if (!Resolve(ent, ref psionics, logMissing: true)
        //     || !psionics.ActivePowers.Contains(NoosphericZap))
        //     return;

        // var actionTarget = Comp<EntityTargetActionComponent>(action.Value);
        // if (actionTarget.Cooldown is {} cooldown && cooldown.End > _timing.CurTime
        //     || !TryComp<NPCRangedCombatComponent>(ent, out var combat)
        //     || !_actions.ValidateEntityTarget(ent, combat.Target, (action.Value, actionTarget))
        //     || actionTarget.Event is not {} ev)
        //     return;

        // ev.Target = combat.Target;
        // _actions.PerformAction(ent, null, action.Value, actionTarget, ev, _timing.CurTime, predicted: false);
    }
}
