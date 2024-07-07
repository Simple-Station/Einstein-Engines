using Content.Shared.Vehicle;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.GameStates;

namespace Content.Client.Vehicle
{
    public sealed class ClientVehicleSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RiddenVehicleComponent, ComponentHandleState>(OnHandleState);
        }

        private void OnHandleState(EntityUid uid, RiddenVehicleComponent component, ref ComponentHandleState args)
        {
            if (args.Current is not RiddenVehicleComponentState state)
                return;

            component.Riders.Clear();
            component.Riders.UnionWith(state.Riders);
        }
    }
}
