using Content.Server.Hands.Systems;
using Content.Shared.Construction;
using Content.Shared.Hands.Components;
using JetBrains.Annotations;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Construction.Completions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class EmptyAllContainers : IGraphAction
    {
        /// <summary>
        ///     Whether or not the user should attempt to pick up the removed entities.
        /// </summary>
        [DataField]
        public bool Pickup = false;

        /// <summary>
        ///    Whether or not to empty the container at the user's location.
        /// </summary>
        [DataField]
        public bool EmptyAtUser = false;

        public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
        {
            if (!entityManager.TryGetComponent(uid, out ContainerManagerComponent? containerManager))
                return;

            var containerSystem = entityManager.EntitySysManager.GetEntitySystem<ContainerSystem>();
            var handSystem = entityManager.EntitySysManager.GetEntitySystem<HandsSystem>();
            var transformSystem = entityManager.EntitySysManager.GetEntitySystem<TransformSystem>();

            HandsComponent? hands = null;
            var pickup = Pickup && entityManager.TryGetComponent(userUid, out hands);

            foreach (var container in containerSystem.GetAllContainers(uid, containerManager))
            {
                foreach (var ent in containerSystem.EmptyContainer(container, true, reparent: !pickup))
                {
                    if (EmptyAtUser && userUid is not null)
                        transformSystem.DropNextTo(ent, (EntityUid) userUid);

                    if (pickup)
                        handSystem.PickupOrDrop(userUid, ent, handsComp: hands);
                }
            }
        }
    }
}
