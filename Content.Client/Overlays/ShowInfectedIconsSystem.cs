using System.Linq;
using Content.Shared.Aliens.Components;
using Content.Shared.Mobs;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.Overlays;

/// <summary>
/// This handles...
/// </summary>
public sealed class ShowInfectedIconsSystem : EquipmentHudSystem<ShowInfectedIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlienInfectedComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, AlienInfectedComponent component, ref GetStatusIconsEvent ev)
    {
        if (!IsActive)
            return;
        if (component.GrowthStage <= 5)
        {
            if (_prototype.TryIndex(component.InfectedIcons.ElementAt(component.GrowthStage), out var iconPrototype))
                ev.StatusIcons.Add(iconPrototype);
        }
        else
        {
            if (_prototype.TryIndex(component.InfectedIcons.ElementAt(5), out var iconPrototype))
                ev.StatusIcons.Add(iconPrototype);
        }

    }
}
