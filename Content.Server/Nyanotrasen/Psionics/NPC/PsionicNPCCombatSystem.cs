using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Server.NPC.Events;
using Content.Server.NPC.Components;
using Content.Server.Abilities.Psionics;
using Robust.Shared.Timing;

namespace Content.Server.Psionics.NPC
{
    public sealed class PsionicNPCCombatSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<NoosphericZapPowerComponent, NPCSteeringEvent>(ZapCombat);
        }

        private void ZapCombat(EntityUid uid, NoosphericZapPowerComponent component, ref NPCSteeringEvent args)
        {
            if (!TryComp<ActionsComponent>(uid, out var actions))
                return;

            // two years and one action rework later and npcs still can't directly use actions so we have to do the checks ourselves
            if (!_actions.TryGetActionData(component.NoosphericZapActionEntity, out var actionData))
                return;

            if (!TryComp<EntityTargetActionComponent>(component.NoosphericZapActionEntity, out var entTarget))
                return;

            if (actionData.Cooldown.HasValue && actionData.Cooldown.Value.End > _timing.CurTime)
                return;

            if (!TryComp<NPCRangedCombatComponent>(uid, out var combat))
                return;

            if (_actions.ValidateEntityTarget(uid, combat.Target, entTarget))
            {
                var ev = entTarget.Event;

                _actions.PerformAction(uid, actions, (EntityUid) component.NoosphericZapActionEntity, entTarget, entTarget.Event, _timing.CurTime, false);
            }
        }
    }
}
