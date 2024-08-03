using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared.Showers
{
    public abstract class SharedShowerSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ShowerComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<ShowerComponent, GetVerbsEvent<AlternativeVerb>>(OnToggleShowerVerb);
            SubscribeLocalEvent<ShowerComponent, ActivateInWorldEvent>(OnActivateInWorld);
        }
        private void OnMapInit(EntityUid uid, ShowerComponent component, MapInitEvent args)
        {
            if (_random.Prob(0.5f))
                component.ToggleShower = true;
            UpdateAppearance(uid);
        }
        private void OnToggleShowerVerb(EntityUid uid, ShowerComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess || args.Hands == null)
                return;

            AlternativeVerb toggleVerb = new()
            {
                Act = () => ToggleShowerHead(uid, args.User, component)
            };

            if (component.ToggleShower)
            {
                toggleVerb.Text = Loc.GetString("shower-turn-on");
                toggleVerb.Icon =
                    new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/open.svg.192dpi.png"));
            }
            else
            {
                toggleVerb.Text = Loc.GetString("shower-turn-off");
                toggleVerb.Icon =
                    new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/close.svg.192dpi.png"));
            }
            args.Verbs.Add(toggleVerb);
        }
        private void OnActivateInWorld(EntityUid uid, ShowerComponent comp, ActivateInWorldEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            ToggleShowerHead(uid, args.User, comp);
        }
        public void ToggleShowerHead(EntityUid uid, EntityUid? user = null, ShowerComponent? component = null, MetaDataComponent? meta = null)
        {
            if (!Resolve(uid, ref component))
                return;

            component.ToggleShower = !component.ToggleShower;

            UpdateAppearance(uid, component);
        }
        private void UpdateAppearance(EntityUid uid, ShowerComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            _appearance.SetData(uid, ShowerVisuals.ShowerVisualState, component.ToggleShower ? ShowerVisualState.Off : ShowerVisualState.On);
        }
    }
}