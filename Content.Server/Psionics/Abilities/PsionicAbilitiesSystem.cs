using Content.Shared.Psionics.Abilities;
using Content.Shared.Actions;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.StatusEffect;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Content.Shared.Popups;
using Robust.Shared.Serialization.Manager;

namespace Content.Server.Psionics.Abilities
{
    public sealed class PsionicAbilitiesSystem : EntitySystem
    {
        [Dependency] private readonly IComponentFactory _componentFactory = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
        [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;

        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Adds a psychic power once a character rolls one. This used to be a system you have to select for. However the opt-in is no longer the text window, but is now done at character creation.
        /// TODO: This is going to get removed when I reach Part 3 of my reworks, when I touch upon the GlimmerSystem itself and overhaul how players get powers.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="component"></param>
        /// <param name="args"></param>
        public void AddPsionics(EntityUid uid)
        {
            if (Deleted(uid))
                return;

            AddRandomPsionicPower(uid);

        }
        public void AddRandomPsionicPower(EntityUid uid)
        {
            EnsureComp<PsionicComponent>(uid, out var psionic);

            if (!_prototypeManager.TryIndex<WeightedRandomPrototype>("RandomPsionicPowerPool", out var pool))
            {
                Logger.Error("Can't index the random psionic power pool!");
                return;
            }

            var newPool = _serialization.CreateCopy(pool, null, false, true);
            foreach (var component in pool.Weights.Keys)
            {
                var checkedComponent = _componentFactory.GetComponent(component);
                if (EntityManager.HasComponent(uid, checkedComponent.GetType()))
                    newPool.Weights.Remove(component);
            }

            if (newPool.Weights.Keys != null)
            {
                var newComponent = _componentFactory.GetComponent(newPool.Pick());

                EntityManager.AddComponent(uid, newComponent);
                _glimmerSystem.DeltaGlimmerInput(_random.NextFloat(psionic.Amplification * psionic.Dampening, psionic.Amplification * psionic.Dampening * 5));
            }
            return;
        }

        public void RemovePsionics(EntityUid uid)
        {
            if (RemComp<PotentialPsionicComponent>(uid))
            {
                _popups.PopupEntity(Loc.GetString("mindbreaking-feedback", ("entity", uid)), uid, PopupType.Medium);
                EnsureComp<PsionicInsulationComponent>(uid, out var insul);
                insul.MindBroken = true;
            }

            if (!TryComp<PsionicComponent>(uid, out var psionic))
                return;

            if (!psionic.Removable)
                return;

            if (!_prototypeManager.TryIndex<WeightedRandomPrototype>("RandomPsionicPowerPool", out var pool))
            {
                Logger.Error("Can't index the random psionic power pool!");
                return;
            }

            foreach (var compName in pool.Weights.Keys)
            {
                // component moment
                var comp = _componentFactory.GetComponent(compName);
                if (EntityManager.TryGetComponent(uid, comp.GetType(), out var psionicPower))
                    RemComp(uid, psionicPower);
            }
            if (psionic.PsionicAbility != null){
                _actionsSystem.TryGetActionData( psionic.PsionicAbility, out var psiAbility );
                if (psiAbility != null){
                    _actionsSystem.RemoveAction(uid, psiAbility.Owner);
                }
            }

            _statusEffectsSystem.TryAddStatusEffect(uid, "Stutter", TimeSpan.FromMinutes(5), false, "StutteringAccent");

            _glimmerSystem.DeltaGlimmerOutput(-_random.NextFloat(psionic.Amplification * psionic.Dampening * 5, psionic.Amplification * psionic.Dampening * 10));
            RemComp<PsionicComponent>(uid);
            RemComp<PotentialPsionicComponent>(uid);
        }
    }
}
