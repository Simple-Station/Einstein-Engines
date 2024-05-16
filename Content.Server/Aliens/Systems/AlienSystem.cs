using Content.Server.Actions;
using Content.Server.Aliens.Systems;
using Content.Server.Animals.Components;
using Content.Server.Popups;
using Content.Shared.Aliens;
using Content.Shared.Aliens.Components;
using Content.Shared.Aliens.Systems;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Devour;
using Content.Shared.Devour.Components;
using Content.Shared.DoAfter;
using Content.Shared.Item;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Tag;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using AlienComponent = Content.Shared.Aliens.Components.AlienComponent;

namespace Content.Server.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class AlienSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPlasmaVesselSystem _plasmaVessel = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlienComponent, PickupAttemptEvent>(OnPickup);
        SubscribeLocalEvent<AlienComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AlienComponent, WeednodeActionEvent>(OnNode);
    }

    private void OnMapInit(EntityUid uid, AlienComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.ToggleLightingActionEntity, component.ToggleLightingAction);
        _actions.AddAction(uid, ref component.WeednodeActionEntity, component.WeednodeAction);
    }

    private void OnPickup(EntityUid uid, AlienComponent component, PickupAttemptEvent args)
    {
        if (!_tag.HasTag(args.Item, "AlienItem"))
        {
            args.Cancel();
            _popup.PopupEntity(Loc.GetString("alien-pickup-item-fail"), uid, uid);
        }
    }

    private void OnNode(EntityUid uid, AlienComponent component, WeednodeActionEvent args)
    {
        if (TryComp<PlasmaVesselComponent>(uid, out var plasmaComp)
            && plasmaComp.Plasma < component.PlasmaCostNode)
        {
            _popup.PopupClient(Loc.GetString(Loc.GetString("alien-action-fail-plasma")), uid, uid);
            return;
        }
        CreateStructure(uid, component);
        args.Handled = true;
    }

    public void CreateStructure(EntityUid uid, AlienComponent component)
    {

        if (_container.IsEntityOrParentInContainer(uid))
            return;

        var xform = Transform(uid);
        // Get the tile in front of the drone
        var coords = xform.Coordinates.SnapToGrid(EntityManager, _mapMan);
        var tile = coords.GetTileRef(EntityManager, _mapMan);
        if (tile == null)
            return;

        // Check there are no walls there
        if (_turf.IsTileBlocked(tile.Value, CollisionGroup.Impassable))
        {
            _popup.PopupEntity(Loc.GetString("alien-create-structure-failed"), uid, uid);
            return;
        }

        foreach (var entity in _lookup.GetEntitiesInRange(coords, 0.1f))
        {
            if (Prototype(entity) == null)
                continue;
            if (Prototype(entity)!.ID == component.WeednodePrototype)
                return;
        }

        _plasmaVessel.ChangePlasmaAmount(uid, -component.PlasmaCostNode);
        Spawn(component.WeednodePrototype, _turf.GetTileCenter(tile.Value));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AlienComponent>();

        while (query.MoveNext(out var uid, out var alien))
        {
            var weed = false;
            var passiveDamageComponent = EnsureComp<PassiveDamageComponent>(uid);
            foreach (var entity in _lookup.GetEntitiesInRange(Transform(uid).Coordinates, 0.1f))
            {
                if (HasComp<PlasmaGainModifierComponent>(entity) && passiveDamageComponent.Damage.Empty)
                {
                    passiveDamageComponent.Damage = alien.WeedHeal;
                    weed = true;
                }
            }

            if (!weed)
                passiveDamageComponent.Damage = new DamageSpecifier();
        }
    }

}
