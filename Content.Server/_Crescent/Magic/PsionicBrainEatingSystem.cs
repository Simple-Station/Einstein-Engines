using Content.Server.Storage.EntitySystems;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Magic.Events;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;

namespace Content.Server._Crescent.Magic
{
    public sealed class PsionicBrainEatingSystem : EntitySystem
    {

        [Dependency] private readonly StorageSystem _storage = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicComponent, PsionicPowersModifiedEvent>(OnPowersModified);
        }

        private void OnPowersModified(Entity<PsionicComponent> entity, ref PsionicPowersModifiedEvent args)
        {
            var powers = args.Powers;

            // Get the brain of the entity
        }
    }
}
