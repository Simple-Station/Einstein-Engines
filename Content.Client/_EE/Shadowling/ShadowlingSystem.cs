using Content.Shared._EE.Shadowling.Systems;
using Content.Shared._EE.Shadowling;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._EE.Shadowling;


/// <summary>
/// This handles status icons for slings and thralls
/// </summary>
public sealed class ShadowlingSystem : SharedShadowlingSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrallComponent, GetStatusIconsEvent>(GetThrallIcon);
        SubscribeLocalEvent<ShadowlingComponent, GetStatusIconsEvent>(GetShadowlingIcon);
    }

    private void GetThrallIcon(Entity<ThrallComponent> ent, ref GetStatusIconsEvent args)
    {
        if (HasComp<ShadowlingComponent>(ent))
            return;

        if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }

    private void GetShadowlingIcon(Entity<ShadowlingComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
