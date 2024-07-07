using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Vehicle;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Player;
using Content.Shared.Vehicles.Components;
using Content.Shared.Interaction.Events;


namespace Content.Shared.Vehicle
{
    public sealed class KeySystem : EntitySystem
    {
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<KeyRequiredComponent, InteractUsingEvent>(OnInteractUsing);
            SubscribeLocalEvent<KeyRequiredComponent, AltInteractEvent>(OnAltInteract);
        }

        private void OnInteractUsing(EntityUid uid, KeyRequiredComponent component, InteractUsingEvent args)
        {
            if (args.Handled)
                return;

            if (component.InsertedKey == null && args.Used.HasComponent<KeyComponent>())
            {
                component.InsertedKey = args.Used;
                _popupSystem.PopupEntity("You insert the key.", uid, Filter.Entities(args.User));
                args.Handled = true;
            }
        }

        private void OnAltInteract(EntityUid uid, KeyRequiredComponent component, AltInteractEvent args)
        {
            if (args.Handled)
                return;

            if (component.InsertedKey != null)
            {
                _popupSystem.PopupEntity("You remove the key.", uid, Filter.Entities(args.User));
                component.InsertedKey = null;
                args.Handled = true;
            }
        }
    }
}
