namespace Content.Shared.Cybernetics
{
    public sealed class CyberneticPsionicDisableSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<CyberneticPsionicDisableComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<CyberneticPsionicDisableComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnStartup(EntityUid uid, CyberneticPsionicDisableComponent component, ComponentStartup args)
        {
        }

        private void OnShutdown(EntityUid uid, CyberneticPsionicDisableComponent component, ComponentShutdown args)
        {
        }
    }
}
