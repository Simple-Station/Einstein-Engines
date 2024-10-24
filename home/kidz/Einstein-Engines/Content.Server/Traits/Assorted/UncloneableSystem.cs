using Content.Shared.Cloning;

namespace Content.Server.Traits.Assorted
{
    public sealed class UncloneableSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<UncloneableComponent, AttemptCloningEvent>(OnAttemptCloning);
        }

        private void OnAttemptCloning(EntityUid uid, UncloneableComponent component, ref AttemptCloningEvent args)
        {
            if (args.CloningFailMessage is not null
                || args.Cancelled)
                return;

            args.CloningFailMessage = "cloning-console-uncloneable-trait-error";
            args.Cancelled = true;
        }
    }
}
