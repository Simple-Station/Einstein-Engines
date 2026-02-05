using Content.Shared._Orehum.ME4TA.UI.Event.Components;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._Orehum.ME4TA.UI.Event
{
    public sealed class ShowEventIconSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototype = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<EventIconComponent, GetStatusIconsEvent>(GetEventIcons);
        }

        private void GetEventIcons(Entity<EventIconComponent> ent, ref GetStatusIconsEvent args)
        {
            var iconProto = _prototype.Index(ent.Comp.StatusIcon);
            args.StatusIcons.Add(iconProto);
        }
    }
}
