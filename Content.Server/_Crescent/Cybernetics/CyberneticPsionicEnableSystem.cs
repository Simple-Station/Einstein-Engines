using Content.Server.Popups;
using Content.Shared.Abilities.Psionics;

namespace Content.Shared.Cybernetics
{
    public sealed class CyberneticPsionicEnableSystem : EntitySystem
    {

        [Dependency] private readonly PopupSystem _popup = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<CyberneticPsionicEnableComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<CyberneticPsionicEnableComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnStartup(EntityUid uid, CyberneticPsionicEnableComponent component, ComponentStartup args)
        {
            var bullied = false; // Don't show more then one popup.
            if (TryComp<MindbrokenComponent>(uid, out var mindbroken))
            {
                bullied = true;
                _popup.PopupEntity(Loc.GetString("cybernetics-psionicenable-demindbroken"), uid, uid);
                RemComp<MindbrokenComponent>(uid);
            }

            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                component.HadPsionics = true;
            }
            else
            {
                AddComp<PsionicComponent>(uid);
                if (!bullied)
                {
                    _popup.PopupEntity(Loc.GetString("cybernetics-psionicenable-psionicsenabled"), uid, uid);
                    bullied = true;
                }
            }
        }

        private void OnShutdown(EntityUid uid, CyberneticPsionicEnableComponent component, ComponentShutdown args)
        {
        }
    }
}
