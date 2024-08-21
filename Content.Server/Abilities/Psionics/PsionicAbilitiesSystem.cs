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
        private const string GenericInitializationMessage = "generic-power-initialization-feedback";

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<InnatePsionicPowersComponent, ComponentStartup>(InnatePowerStartup);
            SubscribeLocalEvent<PsionicComponent, ComponentShutdown>(OnPsionicShutdown);
        }

        /// <summary>
        ///     Special use-case for a InnatePsionicPowers, which allows an entity to start with any number of Psionic Powers.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="comp"></param>
        /// <param name="args"></param>
        private void InnatePowerStartup(EntityUid uid, InnatePsionicPowersComponent comp, ComponentStartup args)
        {
            // Any entity with InnatePowers should also be psionic, but in case they aren't already...
            EnsureComp<PsionicComponent>(uid, out var psionic);

            foreach (var proto in comp.PowersToAdd)
                if (!psionic.ActivePowers.Contains(_prototypeManager.Index(proto)))
                    InitializePsionicPower(uid, _prototypeManager.Index(proto), psionic, false);
        }

        private void OnPsionicShutdown(EntityUid uid, PsionicComponent component, ComponentShutdown args)
        {
            if (!EntityManager.EntityExists(uid)
                || HasComp<MindbrokenComponent>(uid))
                return;

            RemoveAllPsionicPowers(uid);
        }

        /// <summary>
        ///     The most shorthand route to creating a Psion. If an entity is not already psionic, it becomes one. This also adds a random new PsionicPower.
        ///     To create a "Latent Psychic"(Psion with no powers) just add or ensure the PsionicComponent normally.
        /// </summary>
        /// <param name="uid"></param>
        public void AddPsionics(EntityUid uid)
        {
            if (Deleted(uid))
                return;

            AddRandomPsionicPower(uid);
        }

        /// <summary>
        ///     Pretty straightforward, adds a random psionic power to a given Entity. If that Entity is not already Psychic, it will be made one.
        ///     If an entity already has all possible powers, this will not add any new ones.
        /// </summary>
        /// <param name="uid"></param>
        public void AddRandomPsionicPower(EntityUid uid)
        {
            // We need to EnsureComp here to make sure that we aren't iterating over a component that:
            // A: Isn't fully initialized
            // B: Is in the process of being shutdown/deleted
            // Imagine my surprise when I found out Resolve doesn't check for that.
            // TODO: This EnsureComp will be 1984'd in a separate PR, when I rework how you get psionics in the first place.
            EnsureComp<PsionicComponent>(uid, out var psionic);

            if (!_prototypeManager.TryIndex<WeightedRandomPrototype>(_pool.Id, out var pool))
                return;

            var newPool = pool.Weights.Keys.ToList();
            newPool.RemoveAll(s =>
                _prototypeManager.TryIndex<PsionicPowerPrototype>(s, out var p) &&
                psionic.ActivePowers.Contains(p));

            if (newPool is null)
                return;

            var newProto = _random.Pick(newPool);
            if (!_prototypeManager.TryIndex<PsionicPowerPrototype>(newProto, out var newPower))
                return;

            InitializePsionicPower(uid, newPower);

            _glimmerSystem.Glimmer += _random.Next(1, (int) Math.Round(1 + psionic.CurrentAmplification + psionic.CurrentDampening));
        }

        /// <summary>
        ///     Initializes a new Psionic Power on a given entity, assuming the entity does not already have said power initialized.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="proto"></param>
        /// <param name="psionic"></param>
        /// <param name="playPopup"></param>
        public void InitializePsionicPower(EntityUid uid, PsionicPowerPrototype proto, PsionicComponent psionic, bool playPopup = true)
        {
            if (!_prototypeManager.HasIndex<PsionicPowerPrototype>(proto.ID)
                || psionic.ActivePowers.Contains(proto))
                return;

            psionic.ActivePowers.Add(proto);

            AddPsionicActions(uid, proto, psionic);
            AddPsionicPowerComponents(uid, proto);
            AddPsionicStatSources(proto, psionic);
            RefreshPsionicModifiers(uid, psionic);

            if (playPopup)
                _popups.PopupEntity(Loc.GetString(GenericInitializationMessage), uid, uid, PopupType.MediumCaution);
            // TODO: Replace this with chat message: _popups.PopupEntity(proto.InitializationFeedback, uid, uid, PopupType.MediumCaution);
        }

        /// <summary>
        ///     Initializes a new Psionic Power on a given entity, assuming the entity does not already have said power initialized.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="proto"></param>
        /// <param name="psionic"></param>
        /// <param name="playPopup"></param>
        public void InitializePsionicPower(EntityUid uid, PsionicPowerPrototype proto, bool playPopup = true)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionic))
                return;

            InitializePsionicPower(uid, proto, psionic, playPopup);
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

        /// <summary>
        ///     A more advanced form of removing powers. Mindbreaking not only removes all psionic powers,
        ///     it also disables the possibility of obtaining new ones.
        /// </summary>
        /// <param name="uid"></param>
        public void MindBreak(EntityUid uid)
        {
            RemoveAllPsionicPowers(uid, true);
        }

        /// <summary>
        ///     Remove all Psionic powers, with accompanying actions, components, and casting stat sources, from a given Psion.
        ///     Optionally, the Psion can also be rendered permanently non-Psionic.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="mindbreak"></param>
        public void RemoveAllPsionicPowers(EntityUid uid, bool mindbreak = false)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionic)
                || !psionic.Removable)
                return;

            RemovePsionicActions(uid, psionic);

            var newPsionic = psionic.ActivePowers.ToList();
            foreach (var proto in newPsionic)
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
                _statusEffectsSystem.TryAddStatusEffect(uid, psionic.MindbreakingStutterCondition,
                    TimeSpan.FromMinutes(psionic.MindbreakingStutterTime * psionic.CurrentAmplification * psionic.CurrentDampening),
                    false,
                    psionic.MindbreakingStutterAccent);

                _popups.PopupEntity(Loc.GetString(psionic.MindbreakingFeedback, ("entity", MetaData(uid).EntityName)), uid, uid, PopupType.MediumCaution);

                RemComp<PsionicComponent>(uid);
                RemComp<InnatePsionicPowersComponent>(uid);
                return;
            }
            RefreshPsionicModifiers(uid, psionic);
        }

        /// <summary>
        ///     Add all actions associated with a specific Psionic Power
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="proto"></param>
        /// <param name="psionic"></param>
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

        /// <summary>
        ///     Add all components associated with a specific Psionic power.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="proto"></param>
        private void AddPsionicPowerComponents(EntityUid uid, PsionicPowerPrototype proto)
        {
            if (proto.Components is null)
                return;

            foreach (var entry in proto.Components.Values)
            {
                if (HasComp(uid, entry.Component.GetType()))
                    continue;

                var comp = (Component) _serialization.CreateCopy(entry.Component, notNullableOverride: true);
                comp.Owner = uid;
                EntityManager.AddComponent(uid, comp);
            }
        }

        /// <summary>
        ///     Update the Amplification and Dampening sources of a Psion to include a new Power.
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="psionic"></param>
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
        private void RemovePsionicActions(EntityUid uid, PsionicComponent psionic)
        {
            if (psionic.Actions is null)
                return;

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
            if (proto.Components is null)
                return;

            foreach (var comp in proto.Components)
            {
                var powerComp = (Component) _componentFactory.GetComponent(comp.Key);
                if (!EntityManager.HasComponent(uid, powerComp.GetType()))
                    continue;

                EntityManager.RemoveComponent(uid, powerComp.GetType());
            }
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
