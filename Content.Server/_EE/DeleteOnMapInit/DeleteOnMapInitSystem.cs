using Content.Shared._Shitmed.Body.Events;

namespace Content.Server._EE.DeleteOnMapInit
{
    public sealed partial class DeleteOnMapInitSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DeleteOnMapInitComponent, MapInitEvent>(OnMapInit);
        }

        private void OnMapInit(EntityUid uid, DeleteOnMapInitComponent comp, MapInitEvent args)
        {
            ///var ev = new AmputateAttemptEvent(uid);
            ///RaiseLocalEvent(uid, ref ev);
            EntityManager.QueueDeleteEntity(uid);
        }
    }
}
