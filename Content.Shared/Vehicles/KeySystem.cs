using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Player;

namespace Content.Shared.Vehicle
{
    public sealed class KeySystem : EntitySystem
    {
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<KeyComponent, InteractHandEvent>(OnInteractHand);
        }

        private void OnInteractHand(EntityUid uid, KeyComponent component, InteractHandEvent args)

        {

            if (args.Handled)

                return;


            if (component.IsInserted)

            {

                component.IsInserted = false;

                _popupSystem.PopupEntity("Key removed.", uid, args.User, PopupType.Medium);

            }

            else

            {

                component.IsInserted = true;

                _popupSystem.PopupEntity("Key inserted.", uid, args.User, PopupType.Medium);

            }


            args.Handled = true;

        }
    }
}
