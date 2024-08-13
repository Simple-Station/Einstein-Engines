using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Server.EUI;
using Content.Server.Psionics;
using Content.Server.Mind;
using Content.Shared.StatusEffect;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Psionics;

namespace Content.Server.Abilities.Psionics
{
    public sealed class PsionicAbilitiesSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly EuiManager _euiManager = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
        [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly MindSystem _mindSystem = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;

        private ISawmill _sawmill = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicAwaitingPlayerComponent, PlayerAttachedEvent>(OnPlayerAttached);
            SubscribeLocalEvent<InnatePsionicPowersComponent, ComponentStartup>(InnatePowerStartup);
        }

        private void InnatePowerStartup(EntityUid uid, InnatePsionicPowersComponent comp, ComponentStartup args)
        {
            //Any entity with InnatePowers should also be psionic, but in case they aren't already...
            EnsureComp<PsionicComponent>(uid);

            foreach (var proto in comp.PowersToAdd)
                InitializePsionicPower(uid, proto, false);
        }
        private void OnPlayerAttached(EntityUid uid, PsionicAwaitingPlayerComponent component, PlayerAttachedEvent args)
        {
            if (TryComp<PsionicBonusChanceComponent>(uid, out var bonus) && bonus.Warn == true)
                _euiManager.OpenEui(new AcceptPsionicsEui(uid, this), args.Player);
            else
                AddRandomPsionicPower(uid);
            RemCompDeferred<PsionicAwaitingPlayerComponent>(uid);
        }

        public void AddPsionics(EntityUid uid, bool warn = true)
        {
            if (Deleted(uid)
                || HasComp<PsionicComponent>(uid))
                return;

            if (!_mindSystem.TryGetMind(uid, out _, out var mind))
            {
                EnsureComp<PsionicAwaitingPlayerComponent>(uid);
                return;
            }

            if (!_mindSystem.TryGetSession(mind, out var client))
                return;

            if (warn && HasComp<ActorComponent>(uid))
                _euiManager.OpenEui(new AcceptPsionicsEui(uid, this), client);
            else
                AddRandomPsionicPower(uid);
        }

        public void AddRandomPsionicPower(EntityUid uid)
        {
            EnsureComp<PsionicComponent>(uid, out var psionic);

            if (!_prototypeManager.TryIndex<WeightedRandomPrototype>("RandomPsionicPowerPool", out var pool))
            {
                _sawmill.Error("Can't index the random psionic power pool!");
                return;
            }

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

            _glimmerSystem.Glimmer += _random.Next(1, 5);
        }

        public void InitializePsionicPower(EntityUid uid, PsionicPowerPrototype proto, bool playPopup = true)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionic)
                || !_prototypeManager.TryIndex(proto.ID, out var _))
                return;

            psionic.ActivePowers.Add(proto);

            foreach (var id in proto.Actions)
            {
                EntityUid? actionId = null;
                if (_actions.AddAction(uid, ref actionId, id))
                {
                    _actions.StartUseDelay(actionId);
                    psionic.Actions.Add((id, actionId));
                }
            }

            if (proto.AmplificationModifier != 0)
                psionic.AmplificationSources.Add(proto.Name, proto.AmplificationModifier);

            if (proto.DampeningModifier != 0)
                psionic.DampeningSources.Add(proto.Name, proto.DampeningModifier);

            if (playPopup)
                _popups.PopupEntity(proto.InitializationFeedback, uid, uid, PopupType.MediumCaution);

            if (proto.Components is not null)
                foreach (var comp in proto.Components)
                    if (!HasComp(uid, comp.GetType()))
                        EntityManager.AddComponent(uid, comp);

            RefreshPsionicModifiers(uid, psionic);
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

        public void RemoveAllPsionicPowers(EntityUid uid, bool mindbreak = false)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionic)
                || !psionic.Removable)
                return;

            var newPsionic = _serialization.CreateCopy(psionic, null, false, true);
            foreach (var proto in newPsionic.ActivePowers)
            {
                if (!_prototypeManager.TryIndex<PsionicPowerPrototype>(proto.ID, out var power))
                    continue;

                if (power.Components is not null)
                    foreach (var comp in power.Components)
                        EntityManager.RemoveComponent(uid, comp);

                if (psionic.Actions is not null)
                    foreach (var action in psionic.Actions)
                        _actionsSystem.RemoveAction(uid, action.Entity);

                // If we're mindbreaking, we can skip the last two enumerations since the PsionicComponent is getting 1984'd.
                if (mindbreak)
                    continue;

                if (power.AmplificationModifier != 0)
                    psionic.AmplificationSources.Remove(power.Name);

                if (power.DampeningModifier != 0)
                    psionic.DampeningSources.Remove(power.Name);
            }

            if (mindbreak)
            {
                RemComp<PsionicComponent>(uid);
                EnsureComp<MindbrokenComponent>(uid);
                _statusEffectsSystem.TryAddStatusEffect(uid, "Stutter", TimeSpan.FromMinutes(5 * psionic.CurrentAmplification * psionic.CurrentDampening), false, "StutteringAccent");
            }
        }

        public void RemovePsionicPower(EntityUid uid, PsionicPowerPrototype proto, bool removedByComponent = false)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionic)
                || !psionic.Removable
                || !_prototypeManager.TryIndex(proto.ID, out var _))
                return;

            if (removedByComponent
                && proto.Components is not null)
                foreach (var comp in proto.Components)
                    EntityManager.RemoveComponent(uid, comp);

            if (psionic.Actions is not null)
                foreach (var action in psionic.Actions)
                    _actionsSystem.RemoveAction(uid, action.Entity);

            if (proto.AmplificationModifier != 0)
                psionic.AmplificationSources.Remove(proto.Name);

            if (proto.DampeningModifier != 0)
                psionic.DampeningSources.Remove(proto.Name);
        }
    }
}
