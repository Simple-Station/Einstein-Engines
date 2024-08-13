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
        [Dependency] private readonly IComponentFactory _componentFactory = default!;
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

            foreach (var (id, entity) in proto.Actions)
            {
                if (entity is null)
                    continue;
                _actions.AddAction(uid, id.Id, entity.Value);
                if (_actions.TryGetActionData(entity.Value, out var _))
                    _actions.StartUseDelay(entity.Value);
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

        public void RemovePsionics(EntityUid uid)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionic)
                || !psionic.Removable)
                return;

            if (!_prototypeManager.TryIndex<WeightedRandomPrototype>("RandomPsionicPowerPool", out var pool))
            {
                _sawmill.Error("Can't index the random psionic power pool!");
                return;
            }

            foreach (var compName in pool.Weights.Keys)
            {
                // component moment
                var comp = _componentFactory.GetComponent(compName);
                if (EntityManager.TryGetComponent(uid, comp.GetType(), out var psionicPower))
                    RemComp(uid, psionicPower);
            }
            if (psionic.PsionicAbility != null
                && _actionsSystem.TryGetActionData(psionic.PsionicAbility, out var psiAbility)
                && psiAbility is not null)
                _actionsSystem.RemoveAction(uid, psionic.PsionicAbility);

            _statusEffectsSystem.TryAddStatusEffect(uid, "Stutter", TimeSpan.FromMinutes(5), false, "StutteringAccent");

            RemComp<PsionicComponent>(uid);
        }
    }
}
