using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.StatusEffect;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Psionics;
using System.Linq;

namespace Content.Server.Abilities.Psionics
{

    public sealed class PsionicAbilitiesSystem : EntitySystem
    {
        [Dependency] private readonly IComponentFactory _componentFactory = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
        [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;

        private ProtoId<WeightedRandomPrototype> _pool = "RandomPsionicPowerPool";

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<InnatePsionicPowersComponent, ComponentStartup>(InnatePowerStartup);
            SubscribeLocalEvent<PsionicComponent, ComponentShutdown>(OnPsionicShutdown);
        }

        /// <summary>
        ///     Updates a Psion's casting stats, call this anytime a system adds a new source of Amp or Damp.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="comp"></param>
        public void RefreshPsionicModifiers(EntityUid uid, PsionicComponent? comp = null)
        {
            if (!Resolve(uid, ref comp))
                return;

            var ampModifier = 0f;
            var dampModifier = 0f;
            foreach (var (_, source) in comp.AmplificationSources)
                ampModifier += source;
            foreach (var (_, source) in comp.DampeningSources)
                dampModifier += source;

            var ev = new OnSetPsionicStatsEvent(ampModifier, dampModifier);
            RaiseLocalEvent(uid, ref ev);
            ampModifier = ev.AmplificationChangedAmount;
            dampModifier = ev.DampeningChangedAmount;

            comp.CurrentAmplification = ampModifier;
            comp.CurrentDampening = dampModifier;
            Dirty(uid, comp);
        }

        public void MindBreak(EntityUid uid)
        {
            RemoveAllPsionicPowers(uid, true);
        }

        public void RemoveAllPsionicPowers(EntityUid uid, bool mindbreak = false)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionicComp)
                || !psionicComp.Removable)
                return;

            RemovePsionicActions(uid, psionicComp);

            var newPsionic = _serialization.CreateCopy(psionicComp, null, false, true);
            foreach (var proto in newPsionic.ActivePowers)
            {
                if (!_prototypeManager.TryIndex<PsionicPowerPrototype>(proto.ID, out var power))
                    continue;

                RemovePsionicPowerComponents(uid, proto);

                // If we're mindbreaking, we can skip the casting stats since the PsionicComponent is getting 1984'd.
                if (!mindbreak)
                    RemovePsionicStatSources(uid, power, psionicComp);
            }

            if (mindbreak)
            {
                EnsureComp<MindbrokenComponent>(uid);
                _statusEffectsSystem.TryAddStatusEffect(uid, psionicComp.MindbreakingStatusEffect, TimeSpan.FromMinutes(5 * psionicComp.CurrentAmplification * psionicComp.CurrentDampening), false, psionicComp.MindbreakingStatusAccent);
                RemComp<PsionicComponent>(uid);
                return;
            }
            RefreshPsionicModifiers(uid, psionicComp);
        }

        public void AddRandomPsionicPower(EntityUid uid, PsionicComponent? psionic = null)
        {
            if (!Resolve(uid, ref psionic))
                return;

            if (!_prototypeManager.TryIndex<WeightedRandomPrototype>(_pool.Id, out var pool))
                return;

            var newPool = pool.Weights.Keys.ToList();
            newPool.RemoveAll(s =>
                _prototypeManager.TryIndex<PsionicPowerPrototype>(s, out var p) &&
                psionic.ActivePowers.Contains(p));

            if (newPool.Count != 0)
            {
                var newPower = _prototypeManager.Index<PsionicPowerPrototype>(pool.Pick());
                InitializePsionicPower(uid, newPower, psionic);
            }

            _glimmerSystem.Glimmer += _random.Next(1, (int) Math.Round(1 + psionic.CurrentAmplification + psionic.CurrentDampening));
        }

        public void InitializePsionicPower(EntityUid uid, PsionicPowerPrototype proto, PsionicComponent? psionic, bool playPopup = true)
        {
            if (!Resolve(uid, ref psionic)
                || !_prototypeManager.HasIndex<PsionicPowerPrototype>(proto.ID))
                return;

            psionic.ActivePowers.Add(proto);

            AddPsionicActions(uid, proto, psionic);
            AddPsionicPowerComponents(uid, proto);
            AddPsionicStatSources(proto, psionic);
            RefreshPsionicModifiers(uid, psionic);

            if (playPopup)
                _popups.PopupEntity("generic-power-initialization-feedback", uid, uid, PopupType.MediumCaution);
            // TODO: Replace this with chat message: _popups.PopupEntity(proto.InitializationFeedback, uid, uid, PopupType.MediumCaution);
        }

        private void OnPsionicShutdown(EntityUid uid, PsionicComponent component, ComponentShutdown args)
        {
            RemovePsionicActions(uid, component);

            var newPsionic = _serialization.CreateCopy(component, null, false, true);
            foreach (var proto in newPsionic.ActivePowers)
            {
                if (!_prototypeManager.HasIndex<PsionicPowerPrototype>(proto.ID))
                    continue;

                RemovePsionicPowerComponents(uid, proto);
            }
        }

        private void InnatePowerStartup(EntityUid uid, InnatePsionicPowersComponent comp, ComponentStartup args)
        {
            //Any entity with InnatePowers should also be psionic, but in case they aren't already...
            EnsureComp<PsionicComponent>(uid, out var psionic);

            foreach (var proto in comp.PowersToAdd)
                InitializePsionicPower(uid, proto, psionic, false);
        }

        private void AddPsionicActions(EntityUid uid, PsionicPowerPrototype proto, PsionicComponent psionic)
        {
            foreach (var id in proto.Actions)
            {
                EntityUid? actionId = null;
                if (_actions.AddAction(uid, ref actionId, id))
                {
                    _actions.StartUseDelay(actionId);
                    psionic.Actions.Add(id, actionId);
                }
            }
        }

        private void AddPsionicPowerComponents(EntityUid uid, PsionicPowerPrototype proto)
        {
            if (proto.Components is not null)
                foreach (var comp in proto.Components)
                    if (!EntityManager.TryGetComponent(uid, _componentFactory.GetComponent(comp.Key).GetType(), out var powerComp)
                        && powerComp is not null)
                        AddComp(uid, powerComp);
        }

        private void AddPsionicStatSources(PsionicPowerPrototype proto, PsionicComponent psionic)
        {
            if (proto.AmplificationModifier != 0)
                psionic.AmplificationSources.Add(proto.Name, proto.AmplificationModifier);

            if (proto.DampeningModifier != 0)
                psionic.DampeningSources.Add(proto.Name, proto.DampeningModifier);
        }

        /// <summary>
        ///     Remove all Psychic Actions listed in an entity's Psionic Component. Unfortunately, removing actions associated with a specific Power Prototype is not supported.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="psionic"></param>
        private void RemovePsionicActions(EntityUid uid, PsionicComponent psionicComp)
        {
            foreach (var action in psionicComp.Actions)
                _actionsSystem.RemoveAction(uid, action.Value);
        }

        /// <summary>
        ///     Remove all Components associated with a specific Psionic Power.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="proto"></param>
        private void RemovePsionicPowerComponents(EntityUid uid, PsionicPowerPrototype proto)
        {
            foreach (var (name, _) in proto.Components)
                EntityManager.RemoveComponent(uid, (Component) _componentFactory.GetComponent(name));
        }

        /// <summary>
        ///     Remove all stat sources associated with a specific Psionic Power.
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="psionic"></param>
        private void RemovePsionicStatSources(EntityUid uid, PsionicPowerPrototype proto, PsionicComponent psionicComp)
        {
            if (proto.AmplificationModifier != 0)
                psionicComp.AmplificationSources.Remove(proto.Name);

            if (proto.DampeningModifier != 0)
                psionicComp.DampeningSources.Remove(proto.Name);

            RefreshPsionicModifiers(uid, psionicComp);
        }
    }
}
