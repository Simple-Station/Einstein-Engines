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
        private const string GenericInitializationMessage = "generic-power-initialization-feedback";

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<InnatePsionicPowersComponent, ComponentStartup>(InnatePowerStartup);
        }

        private void InnatePowerStartup(EntityUid uid, InnatePsionicPowersComponent comp, ComponentStartup args)
        {
            //Any entity with InnatePowers should also be psionic, but in case they aren't already...
            EnsureComp<PsionicComponent>(uid);

            foreach (var proto in comp.PowersToAdd)
                if (_prototypeManager.TryIndex<PsionicPowerPrototype>(proto, out var powerPrototype))
                    InitializePsionicPower(uid, powerPrototype, false);
        }
        public void AddPsionics(EntityUid uid)
        {
            if (Deleted(uid))
                return;

            AddRandomPsionicPower(uid);
        }

        public void AddRandomPsionicPower(EntityUid uid)
        {
            EnsureComp<PsionicComponent>(uid, out var psionic);

            if (!_prototypeManager.TryIndex<WeightedRandomPrototype>(_pool.Id, out var pool))
                return;

            var newPool = _serialization.CreateCopy(pool, null, false, true);
            foreach (var proto in pool.Weights.Keys)
            {
                if (!_prototypeManager.TryIndex<PsionicPowerPrototype>(proto, out var powerPrototype))
                    continue;

                if (psionic.ActivePowers.Contains(powerPrototype))
                    newPool.Weights.Remove(powerPrototype.ID);
            }

            if (newPool.Weights.Keys != null)
            {
                var newPower = _prototypeManager.Index<PsionicPowerPrototype>(newPool.Pick());
                InitializePsionicPower(uid, newPower);
            }

            _glimmerSystem.Glimmer += _random.Next(1, (int) Math.Round(1 + psionic.CurrentAmplification + psionic.CurrentDampening));
        }

        public void InitializePsionicPower(EntityUid uid, PsionicPowerPrototype proto, bool playPopup = true)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionic)
                || !_prototypeManager.HasIndex<PsionicPowerPrototype>(proto.ID))
                return;

            psionic.ActivePowers.Add(proto);

            AddPsionicActions(uid, proto, psionic);
            AddPsionicPowerComponents(uid, proto);
            AddPsionicStatSources(proto, psionic);
            RefreshPsionicModifiers(uid, psionic);

            if (playPopup)
                _popups.PopupEntity(GenericInitializationMessage, uid, uid, PopupType.MediumCaution);
            // TODO: Replace this with chat message: _popups.PopupEntity(proto.InitializationFeedback, uid, uid, PopupType.MediumCaution);
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
                    if (!EntityManager.TryGetComponent(uid, _componentFactory.GetComponent(comp).GetType(), out var powerComp)
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
        ///     Updates a Psion's casting stats, call this anytime a system adds a new source of Amp or Damp.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="comp"></param>
        public void RefreshPsionicModifiers(EntityUid uid, PsionicComponent comp)
        {
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
        }

        /// <summary>
        ///     Updates a Psion's casting stats, call this anytime a system adds a new source of Amp or Damp.
        ///     Variant function for systems that didn't already have the PsionicComponent.
        /// </summary>
        /// <param name="uid"></param>
        public void RefreshPsionicModifiers(EntityUid uid)
        {
            if (!TryComp<PsionicComponent>(uid, out var comp))
                return;

            RefreshPsionicModifiers(uid, comp);
        }

        public void MindBreak(EntityUid uid)
        {
            RemoveAllPsionicPowers(uid, true);
        }

        public void RemoveAllPsionicPowers(EntityUid uid, bool mindbreak = false)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionic)
                || !psionic.Removable)
                return;

            RemovePsionicActions(uid, psionic);

            var newPsionic = _serialization.CreateCopy(psionic, null, false, true);
            foreach (var proto in newPsionic.ActivePowers)
            {
                if (!_prototypeManager.TryIndex<PsionicPowerPrototype>(proto.ID, out var power))
                    continue;

                RemovePsionicPowerComponents(uid, proto);

                // If we're mindbreaking, we can skip the casting stats since the PsionicComponent is getting 1984'd.
                if (!mindbreak)
                    RemovePsionicStatSources(uid, power, psionic);
            }

            if (mindbreak)
            {
                EnsureComp<MindbrokenComponent>(uid);
                _statusEffectsSystem.TryAddStatusEffect(uid, "Stutter", TimeSpan.FromMinutes(5 * psionic.CurrentAmplification * psionic.CurrentDampening), false, "StutteringAccent");
                RemComp<PsionicComponent>(uid);
                return;
            }
            RefreshPsionicModifiers(uid, psionic);
        }

        /// <summary>
        ///     Remove all Psychic Actions listed in an entity's Psionic Component. Unfortunately, removing actions associated with a specific Power Prototype is not supported.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="psionic"></param>
        private void RemovePsionicActions(EntityUid uid, PsionicComponent psionic)
        {
            if (psionic.Actions is not null)
                foreach (var action in psionic.Actions)
                    _actionsSystem.RemoveAction(uid, action.Value);
        }

        /// <summary>
        ///     Remove all Components associated with a specific Psionic Power.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="proto"></param>
        private void RemovePsionicPowerComponents(EntityUid uid, PsionicPowerPrototype proto)
        {
            if (proto.Components is not null)
                foreach (var comp in proto.Components)
                    if (EntityManager.TryGetComponent(uid, _componentFactory.GetComponent(comp).GetType(), out var powerComp)
                        && powerComp is not null)
                        RemComp(uid, powerComp);
        }

        /// <summary>
        ///     Remove all stat sources associated with a specific Psionic Power.
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="psionic"></param>
        private void RemovePsionicStatSources(EntityUid uid, PsionicPowerPrototype proto, PsionicComponent psionic)
        {
            if (proto.AmplificationModifier != 0)
                psionic.AmplificationSources.Remove(proto.Name);

            if (proto.DampeningModifier != 0)
                psionic.DampeningSources.Remove(proto.Name);

            RefreshPsionicModifiers(uid, psionic);
        }
    }
}
