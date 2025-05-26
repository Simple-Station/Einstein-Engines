using Content.Server.Body.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Coordinates;
using Content.Shared.Gibbing.Systems;
using Content.Shared.Inventory;

namespace Content.Shared.Cybernetics
{
    public sealed class InternalBombSystem : EntitySystem
    {

        [Dependency] private readonly SharedActionsSystem _actions = default!;

        [Dependency] private readonly GibbingSystem _gibbing = default!;

        [Dependency] private readonly BodySystem _body = default!;

        [Dependency] private readonly ExplosionSystem _explosion = default!;

        [Dependency] private readonly InventorySystem _inventory = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<InternalBombComponent, InternalBombActivatedEvent>(OnCortexBombActivated);

            SubscribeLocalEvent<InternalBombComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<InternalBombComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnCortexBombActivated(EntityUid uid, InternalBombComponent component, InternalBombActivatedEvent args)
        {

            var items = _inventory.GetHandOrInventoryEntities(uid);
            foreach (var item in items)
            {
                Del(item);
            }
            _body.GibBody(uid, true);

            _explosion.QueueExplosion(uid, ExplosionSystem.DefaultExplosionPrototypeId, 40, 1, 2);
        }

        private void OnStartup(EntityUid uid, InternalBombComponent component, ComponentStartup args)
        {
            _actions.AddAction(uid, ref component.Action, component.ActionPrototype);
        }

        private void OnShutdown(EntityUid uid, InternalBombComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.Action);
        }
    }
}
