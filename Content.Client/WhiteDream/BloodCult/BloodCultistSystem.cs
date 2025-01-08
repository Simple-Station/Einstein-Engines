using System.Numerics;
using Content.Shared.Antag;
using Content.Shared.Ghost;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Content.Shared.WhiteDream.BloodCult;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Components;
using Content.Shared.WhiteDream.BloodCult.Constructs;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Client.WhiteDream.BloodCult;

public sealed class BloodCultistSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PentagramComponent, ComponentStartup>(OnPentagramAdded);
        SubscribeLocalEvent<PentagramComponent, ComponentShutdown>(OnPentagramRemoved);

        SubscribeLocalEvent<ConstructComponent, GetStatusIconsEvent>(OnCanShowConstructIcon);
        SubscribeLocalEvent<BloodCultistComponent, GetStatusIconsEvent>(OnCanShowCultMemberIcon);
        SubscribeLocalEvent<BloodCultLeaderComponent, GetStatusIconsEvent>(OnCanShowCultLeaderIcon);
    }

    private void OnPentagramAdded(EntityUid uid, PentagramComponent component, ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) || sprite.LayerMapTryGet(PentagramKey.Key, out _))
            return;

        var adj = sprite.Bounds.Height / 2 + 1.0f / 32 * 10.0f;

        var randomState = _random.Pick(component.States);

        var layer = sprite.AddLayer(new SpriteSpecifier.Rsi(component.RsiPath, randomState));

        sprite.LayerMapSet(PentagramKey.Key, layer);
        sprite.LayerSetOffset(layer, new(0.0f, adj));
    }

    private void OnPentagramRemoved(EntityUid uid, PentagramComponent component, ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) || !sprite.LayerMapTryGet(PentagramKey.Key, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }

    /// <summary>
    /// Determine whether a client should display the construct icon.
    /// </summary>
    private void OnCanShowConstructIcon(Entity<ConstructComponent> ent, ref GetStatusIconsEvent args)
    {
        if (CanDisplayIcon(ent.Owner, ent.Comp.IconVisibleToGhost))
        {
            if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
                args.StatusIcons.Add(iconPrototype);
        }
    }

    /// <summary>
    /// Determine whether a client should display the cult member icon.
    /// </summary>
    private void OnCanShowCultMemberIcon(Entity<BloodCultistComponent> ent, ref GetStatusIconsEvent args)
    {
        if (CanDisplayIcon(ent.Owner, ent.Comp.IconVisibleToGhost))
        {
            if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
                args.StatusIcons.Add(iconPrototype);
        }
    }

    /// <summary>
    /// Determine whether a client should display the cult leader icon.
    /// </summary>
    private void OnCanShowCultLeaderIcon(Entity<BloodCultLeaderComponent> ent, ref GetStatusIconsEvent args)
    {
        if (CanDisplayIcon(ent.Owner, ent.Comp.IconVisibleToGhost))
        {
            if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
                args.StatusIcons.Add(iconPrototype);
        }
    }

    /// <summary>
    /// The criteria that determine whether a client should see Cult/Cult leader icons.
    /// </summary>
    private bool CanDisplayIcon(EntityUid? uid, bool visibleToGhost)
    {
        if (HasComp<BloodCultistComponent>(uid) || HasComp<BloodCultLeaderComponent>(uid) ||
            HasComp<ConstructComponent>(uid))
            return true;

        return visibleToGhost && HasComp<GhostComponent>(uid);
    }
}
