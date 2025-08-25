using Content.Server.Storage.EntitySystems;
using Content.Shared._Crescent.Magic;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Magic.Events;
using Content.Shared.PowerCell.Components;
using Robust.Server.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;

namespace Content.Server._Crescent.Magic
{
    public sealed class PsionicBrainEatingSystem : EntitySystem
    {

        [Dependency] private readonly ContainerSystem _container = default!;

        // sawmill
        [Dependency] private readonly ILogManager _logManager = default!;
        private ISawmill? _sawmill;
        public override void Initialize()
        {
            base.Initialize();

            _sawmill = _logManager.GetSawmill("fine_dining");

            SubscribeLocalEvent<PsionicComponent, PsionicPowersModifiedEvent>(OnPowersModified);
        }

        private void OnPowersModified(Entity<PsionicComponent> entity, ref PsionicPowersModifiedEvent args)
        {
            var powers = args.Powers;

            if (_sawmill == null)
                _sawmill = _logManager.GetSawmill("fine_dining");

            // Get the torso of the entity
            if (!_container.TryGetContainer(entity.Owner, "body_root_part", out var torsoContainer))
                return;

            _sawmill.Info("step 1");

            if (torsoContainer.ContainedEntities.Count == 0)
                return;

            var torso = torsoContainer.ContainedEntities[0];

            _sawmill.Info("step 1.5");

            // Get the head of the entity
            if (!_container.TryGetContainer(torso, "body_part_slot_head", out var headContainer))
                return;

            _sawmill.Info("step 2 (3??)");

            if (headContainer.ContainedEntities.Count == 0)
                return;

            var head = headContainer.ContainedEntities[0];

            _sawmill.Info("step 3 (4???) [let me leave this hell]");

            // NOW we can get the brain (jesus finally)
            if (!_container.TryGetContainer(head, "body_organ_slot_brain", out var brain))
                return;

            _sawmill.Info("step 4 (5???) [almost there]");

            if (brain.ContainedEntities.Count == 0)
                return;

            var brainEntity = brain.ContainedEntities[0];

            _sawmill.Info("step 5 (6???) [i can see the light]");

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
