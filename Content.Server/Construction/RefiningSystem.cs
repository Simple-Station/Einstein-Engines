using Content.Server.Construction.Components;
using Content.Server.Stack;
using Content.Shared.Construction;
using Content.Shared.Interaction;
using Content.Shared.Storage;
using SharedToolSystem = Content.Shared.Tools.Systems.SharedToolSystem;

namespace Content.Server.Construction
{
    public sealed class RefiningSystem : EntitySystem
    {
        [Dependency] private readonly SharedToolSystem _toolSystem = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<WelderRefinableComponent, InteractUsingEvent>(OnInteractUsing);
            SubscribeLocalEvent<WelderRefinableComponent, WelderRefineDoAfterEvent>(OnDoAfter);
        }

        private void OnInteractUsing(EntityUid uid, WelderRefinableComponent component, InteractUsingEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = _toolSystem.UseTool(args.Used, args.User, uid, component.RefineTime, component.QualityNeeded, new WelderRefineDoAfterEvent());
        }

        private void OnDoAfter(EntityUid uid, WelderRefinableComponent component, WelderRefineDoAfterEvent args)
        {
            if (args.Cancelled)
                return;

            // get last owner coordinates and delete it
            var resultPosition = Transform(uid).Coordinates;
            EntityManager.DeleteEntity(uid);

            // spawn each result after refine
            foreach (var ent in EntitySpawnCollection.GetSpawns(component.RefineResult))
            {
                Spawn(ent, resultPosition);
            }
        }
    }
}
