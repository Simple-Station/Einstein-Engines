using Content.Server.Body.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Coordinates;
using Content.Shared.Gibbing.Systems;
using Content.Shared.Inventory;

namespace Content.Shared.Cybernetics
{
    public sealed class CortexBombSystem : EntitySystem
    {

        [Dependency] private readonly SharedActionsSystem _actions = default!;

        [Dependency] private readonly GibbingSystem _gibbing = default!;

        [Dependency] private readonly BodySystem _body = default!;

        [Dependency] private readonly ExplosionSystem _explosion = default!;

        [Dependency] private readonly InventorySystem _inventory = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CortexBombComponent, CortexBombActivatedEvent>(OnCortexBombActivated);

            SubscribeLocalEvent<CortexBombComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<CortexBombComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnCortexBombActivated(EntityUid uid, CortexBombComponent component, CortexBombActivatedEvent args)
        {

            var items = _inventory.GetHandOrInventoryEntities(uid);
            foreach (var item in items)
            {
                Del(item);
            }
            _body.GibBody(uid, true);

            _explosion.QueueExplosion(uid, ExplosionSystem.DefaultExplosionPrototypeId, 40, 1, 2);
        }

        private void OnStartup(EntityUid uid, CortexBombComponent component, ComponentStartup args)
        {
            _actions.AddAction(uid, ref component.Action, component.ActionPrototype);
        }

        private void OnShutdown(EntityUid uid, CortexBombComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.Action);
        }
    }
}
