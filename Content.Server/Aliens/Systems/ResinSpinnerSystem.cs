using System.Linq;
using System.Numerics;
using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Aliens.Components;
using Content.Shared.Aliens.Systems;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class ResinSpinnerSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPlasmaVesselSystem _plasmaVessel = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResinSpinnerComponent, ResinWallDoAfterEvent>(OnWallDoAfter);
        SubscribeLocalEvent<ResinSpinnerComponent, ResinWindowDoAfterEvent>(OnWindowDoAfter);
        SubscribeLocalEvent<ResinSpinnerComponent, AlienNestDoAfterEvent>(OnNestDoAfter);
    }

    private void OnWallDoAfter(EntityUid uid, ResinSpinnerComponent component, ResinWallDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || component.Deleted)
            return;

        CreateStructure(uid, component, component.WallPrototype);
        args.Handled = true;
    }

    private void OnWindowDoAfter(EntityUid uid, ResinSpinnerComponent component, ResinWindowDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || component.Deleted)
            return;

        CreateStructure(uid, component, component.WindowPrototype);
        args.Handled = true;
    }

    private void OnNestDoAfter(EntityUid uid, ResinSpinnerComponent component, AlienNestDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || component.Deleted)
            return;

        CreateStructure(uid, component, component.NestPrototype, false);
        args.Handled = true;
    }

    public void CreateStructure(EntityUid uid, ResinSpinnerComponent component, string structurePrototype, bool offset = true)
    {

        if (_container.IsEntityOrParentInContainer(uid))
            return;

        var xform = Transform(uid);
        var offsetValue = new Vector2(0, 0);
        if (offset)
            offsetValue = xform.LocalRotation.ToWorldVec();

        var coords = xform.Coordinates.Offset(offsetValue).SnapToGrid(EntityManager, _mapMan);
        var tile = coords.GetTileRef(EntityManager, _mapMan);
        if (tile == null)
            return;

        // Check there are no walls there
        if (_turf.IsTileBlocked(tile.Value, CollisionGroup.Impassable))
        {
            _popupSystem.PopupEntity(Loc.GetString("alien-create-structure-failed"), uid, uid);
            return;
        }

        // Check there are no mobs there
        if (offset)
        {
            if (_lookupSystem.GetLocalEntitiesIntersecting(tile.Value, 0f).Any(entity => HasComp<MobStateComponent>(entity) && entity != uid))
            {
                _popupSystem.PopupEntity(Loc.GetString("alien-create-structure-failed"), uid, uid);
                return;
            }
        }

        foreach (var entity in _lookupSystem.GetEntitiesInRange(coords, 0.1f))
        {
            if (Prototype(entity) == null)
                continue;
            if (Prototype(entity)!.ID != structurePrototype)
                continue;
            _popupSystem.PopupEntity(Loc.GetString("alien-create-structure-failed"), uid, uid);
            return;
        }
        // Make sure we set the invisible wall to despawn properly
        Spawn(structurePrototype, _turf.GetTileCenter(tile.Value));
        _plasmaVessel.ChangePlasmaAmount(uid, -component.PlasmaCostWall);
    }
}
