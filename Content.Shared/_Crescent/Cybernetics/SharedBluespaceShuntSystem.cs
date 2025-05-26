using Content.Shared.Actions;
using Content.Shared.Coordinates;
using Content.Shared.EventScheduler;
using Content.Shared.Examine;
using Content.Shared.Gibbing.Systems;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Shared.Cybernetics
{
    public sealed class SharedBluespaceShuntSystem : EntitySystem
    {

        [Dependency] private readonly ExamineSystemShared _examine = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BluespaceShuntComponent, BluespaceShuntEvent>(OnBluespaceShunt);
        }



        private void OnBluespaceShunt(Entity<BluespaceShuntComponent> ent, ref BluespaceShuntEvent args)
        {
            if (!_timing.IsFirstTimePredicted)
                return;
            var (uid, comp) = ent;
            var user = args.Performer;

            if (comp.OnCooldown)
            {
                _popup.PopupPredicted(Loc.GetString("shunt-cooldown"), user, user);
                return;
            }

            var origin = _transform.GetMapCoordinates(user);
            var target = args.Target.ToMap(EntityManager, _transform);
            if (!_examine.InRangeUnOccluded(origin, target, SharedInteractionSystem.MaxRaycastRange, null))
            {
                // can only dash if the destination is visible on screen
                _popup.PopupClient(Loc.GetString("shunt-unseen"), user, user);
                return;
            }

            var xform = Transform(user);
            _transform.SetCoordinates(user, xform, args.Target);
            _transform.AttachToGridOrMap(user, xform);
            comp.OnCooldown = true;

            var ev = new BluespaceShuntUsedEvent(comp);
            RaiseLocalEvent(uid, ev);

            args.Handled = true;
        }
    }
}
