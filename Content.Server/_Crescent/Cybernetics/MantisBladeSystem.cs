using Content.Server.Body.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.Coordinates;
using Content.Shared.Gibbing.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.Cybernetics
{
    public sealed class MantisBladeSystem : EntitySystem
    {

        [Dependency] private readonly SharedActionsSystem _actions = default!;

        [Dependency] private readonly GibbingSystem _gibbing = default!;

        [Dependency] private readonly BodySystem _body = default!;

        [Dependency] private readonly SharedHandsSystem _hands = default!;
        [Dependency] private readonly PopupSystem _popup = default!;

        [Dependency] private readonly IRobustRandom _rand = default!;
        [Dependency] private readonly AudioSystem _audio = default!;

        [Dependency] private readonly ExplosionSystem _explosion = default!;

        [Dependency] private readonly InventorySystem _inventory = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MantisBladeComponent, MantisBladeToggledEvent>(OnMantisBladeToggled);

            SubscribeLocalEvent<MantisBladeComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<MantisBladeComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnMantisBladeToggled(EntityUid uid, MantisBladeComponent component, MantisBladeToggledEvent args)
        {
            if (!TryToggleItem(uid, component.SwordPrototype, component, out _))
                return;

            List<SoundSpecifier?> soundPool = new()
            {
                new SoundPathSpecifier("/Audio/Effects/gib1.ogg"),
                new SoundPathSpecifier("/Audio/Effects/gib2.ogg"),
                new SoundPathSpecifier("/Audio/Effects/gib3.ogg"),
            };

            var rand = _rand.Next(0, soundPool.Count - 1);
            var sound = soundPool.ToArray()[rand];
            _audio.PlayPvs(sound, uid, AudioParams.Default.WithVolume(-3f));
        }

        private void OnStartup(EntityUid uid, MantisBladeComponent component, ComponentStartup args)
        {
            _actions.AddAction(uid, ref component.Action, component.ActionPrototype);
        }

        private void OnShutdown(EntityUid uid, MantisBladeComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.Action);
        }

        public bool TryToggleItem(EntityUid uid, EntProtoId proto, MantisBladeComponent comp, out EntityUid? equipment)
        {
            equipment = null;
            if (!comp.Equipment.TryGetValue(proto.Id, out var item))
            {
                item = Spawn(proto, Transform(uid).Coordinates);
                if (!_hands.TryForcePickupAnyHand(uid, (EntityUid) item))
                {
                    _popup.PopupEntity(Loc.GetString("changeling-fail-hands"), uid, uid);
                    QueueDel(item);
                    return false;
                }
                comp.Equipment.Add(proto.Id, item);
                equipment = item;
                return true;
            }

            QueueDel(item);
            // assuming that it exists
            comp.Equipment.Remove(proto.Id);

            return true;
        }
    }
}
