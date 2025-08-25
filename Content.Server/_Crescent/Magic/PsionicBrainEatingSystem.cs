using Content.Server.Abilities.Psionics;
using Content.Server.Nutrition;
using Content.Server.Psionics;
using Content.Server.Storage.EntitySystems;
using Content.Shared._Crescent.Magic;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Magic.Events;
using Content.Shared.PowerCell.Components;
using Content.Shared.Psionics;
using Robust.Server.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._Crescent.Magic
{
    public sealed class PsionicBrainEatingSystem : EntitySystem
    {

        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly ContainerSystem _container = default!;
        [Dependency] private readonly PsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        // sawmill
        [Dependency] private readonly ILogManager _logManager = default!;
        private ISawmill? _sawmill;
        public override void Initialize()
        {
            base.Initialize();

            _sawmill = _logManager.GetSawmill("fine_dining");

            SubscribeLocalEvent<PsionicComponent, PsionicPowersModifiedEvent>(OnPowersModified);
            SubscribeLocalEvent<PsionicBrainComponent, BeforeFullyEatenEvent>(OnBrainEaten);
        }

        private void OnBrainEaten(Entity<PsionicBrainComponent> entity, ref BeforeFullyEatenEvent args)
        {
            if (_sawmill == null)
                _sawmill = _logManager.GetSawmill("fine_dining");

            if (HasComp<MindbrokenComponent>(args.User))
                return;

            if (!TryComp<PsionicComponent>(args.User, out var psiComp) && entity.Comp.RequiresLatent)
                return;

            foreach (var power in entity.Comp.PowerPrototypes)
            {
                if (_random.Prob(entity.Comp.ChancePerPower))
                    continue;

                if (!_prototypeManager.TryIndex<PsionicPowerPrototype>(power, out var powerProto))
                    return;

                _psionics.InitializePsionicPower(args.User, powerProto);
            }
        }

        private void OnPowersModified(Entity<PsionicComponent> entity, ref PsionicPowersModifiedEvent args)
        {
            var powers = args.Powers;

            if (_sawmill == null)
                _sawmill = _logManager.GetSawmill("fine_dining");

            // Get the torso of the entity
            if (!_container.TryGetContainer(entity.Owner, "body_root_part", out var torsoContainer))
                return;

            if (torsoContainer.ContainedEntities.Count == 0)
                return;

            var torso = torsoContainer.ContainedEntities[0];

            // Get the head of the entity
            if (!_container.TryGetContainer(torso, "body_part_slot_head", out var headContainer))
                return;

            if (headContainer.ContainedEntities.Count == 0)
                return;

            var head = headContainer.ContainedEntities[0];

            // NOW we can get the brain (jesus finally)
            if (!_container.TryGetContainer(head, "body_organ_slot_brain", out var brain))
                return;

            if (brain.ContainedEntities.Count == 0)
                return;

            var brainEntity = brain.ContainedEntities[0];

            // FINALLY, add PsionicBrainComponent to the brain if it doesn't already exist, and set its powers.

            var psibraincomp = EnsureComp<PsionicBrainComponent>(brainEntity);

            psibraincomp.PowerPrototypes = new List<string>();

            foreach (var power in powers)
            {
                psibraincomp.PowerPrototypes.Add(power.ID);
            }

            // Why did I do this.
        }
    }
}
