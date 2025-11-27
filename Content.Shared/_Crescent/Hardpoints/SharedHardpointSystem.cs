using System.Reflection.Metadata.Ecma335;
using Content.Shared.Construction;
using Content.Shared.Construction.Components;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Destructible;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;

namespace Content.Shared._Crescent.Hardpoints;

/// <summary>
/// This handles...
/// </summary>
public class SharedHardpointSystem : EntitySystem
{
    [Dependency] public readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] public readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] public readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityManager _entMan = default!;

    //used for logging, don't touch this
    private ISawmill _sawmill = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<HardpointAnchorableOnlyComponent, AnchorStateChangedEvent>(OnAnchorChange);
        SubscribeLocalEvent<HardpointAnchorableOnlyComponent, MapInitEvent>(OnMapLoad);
        SubscribeLocalEvent<HardpointComponent, AnchorStateChangedEvent>(OnHardpointAnchor);
        SubscribeLocalEvent<HardpointAnchorableOnlyComponent, ComponentRemove>(OnShipgunRemove);
        // TODO: ACCOUNT FOR REMOVING IT IN ADMIN MODE
        _sawmill = IoCManager.Resolve<ILogManager>().GetSawmill("crescent.hardpoints");
    }

    public void OnMapLoad(EntityUid uid, HardpointAnchorableOnlyComponent comp, ref MapInitEvent args)
    {
        if (Transform(uid).MapUid == null)
            return;
        if (TryAnchorToHardpoint(uid, comp))
            return;
        _sawmill.Debug(
            $"Hardpoint-only weapon had no hardpoint under itself at mapInit. {uid} , {MetaData(uid).EntityName}");
    }
    public void OnAnchorChange(EntityUid uid, HardpointAnchorableOnlyComponent component, ref AnchorStateChangedEvent args)
    {
        //this is here to prevent this code from running at the start of the round OR when you spawn a new ship.
        //otherwise, targeting computers do not see turrets. only sometimes.
        if (_entMan.GetComponent<MetaDataComponent>(uid).EntityLifeStage != EntityLifeStage.MapInitialized)
            return;
        //_sawmill.Debug("ON ANCHOR CHANGE RAN" + args.Anchored.ToString());
        //LOGIC:
        /*
        "im a shipgun"
        "i just got anchored or deanchored!"
        "if i got anchored, let me check if I've got a valid hardpoint under me."
            "if yes, then set the values properly and stay anchored."
            "if no, then and send a popup. the values were never set, so the gun can't fire anyway."
        "if i just got deanchored,"
            "de-set all the values, then de-anchor the gun too."
        */
        if (args.Anchored)
        {
            if (TryAnchorToHardpoint(uid, component)) //if it's a valid hardpoint, then we're good. this function also sets the values properly.
            {
                return;
            }
            else
            {
                //_transformSystem.Unanchor(uid); //if it's not / we dont have a hardpoint under it, kick that shit out
                //play sound effect
                _popup.PopupPredicted(Loc.GetString("WARNING! This weapon is not mounted on a compatible hardpoint and will not function!"), uid, null);
                return;
            }
        }

        //else, if we UNanchored
        if (component.anchoredTo == null) //this should literally never happen
        {
            return;
        }

        var gridUid = Transform(component.anchoredTo.Value).GridUid;
        if (gridUid is null)
            return;

        Deanchor(uid, component.anchoredTo.Value, gridUid.Value, component); //otherwise, kick that shit out
        _transformSystem.Unanchor(uid);

    }

    public void OnHardpointAnchor(EntityUid target, HardpointComponent comp, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored)
            return;
        if (comp.anchoring is null)
            return;
        _transformSystem.Unanchor(comp.anchoring.Value);
    }

    public void Deanchor(EntityUid target, EntityUid anchor, EntityUid grid, HardpointAnchorableOnlyComponent component)
    {
        //_sawmill.Debug("DEANCHOR RAN");
        if (component.anchoredTo is null)
        {
            //_sawmill.Debug($"SharedHardpointSystem had a anchored entity that wasn't attached to a hardpoint!");
            return;
        }
        var hardpointComp = Comp<HardpointComponent>(component.anchoredTo.Value);
        var hardpointUid = component.anchoredTo.Value;
        hardpointComp.anchoring = null;
        HardpointCannonDeanchoredEvent arg = new();
        arg.CannonUid = target;
        arg.gridUid = grid;
        RaiseLocalEvent(hardpointUid, arg);
        component.anchoredTo = null;
        DirtyEntity(target);
        DirtyEntity(anchor);
        //Dirty(arg.CannonUid, component);
    }

    /// <summary>
    /// this is used for when shipguns are destroyed, but this ALSO runs when the grid is deleted. 
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <param name="args"></param>
    public void OnShipgunRemove(EntityUid uid, HardpointAnchorableOnlyComponent component, ComponentRemove args)
    {
        if (component.anchoredTo is null)
        {
            return;
        }
        var gridUid = Transform(component.anchoredTo.Value).GridUid;
        if (gridUid is null)
            return;
        var hardpointComp = Comp<HardpointComponent>(component.anchoredTo.Value);
        var hardpointUid = component.anchoredTo.Value;
        hardpointComp.anchoring = null;
        HardpointCannonDeanchoredEvent arg = new();
        arg.CannonUid = uid;
        arg.gridUid = gridUid.Value;
        RaiseLocalEvent(hardpointUid, arg);
        component.anchoredTo = null;
        DirtyEntity(uid);
        DirtyEntity(hardpointUid);
        //Dirty(arg.CannonUid, component);
    }


    /// <summary>
    /// Returns true/false based on if we are able to anchor something here or not.
    /// TRUE: we're good to anchor here, hardpoint fits/exists.
    /// FALSE: there is no hardpoint/it's of the wrong type
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public bool TryAnchorToHardpoint(EntityUid uid, HardpointAnchorableOnlyComponent component)
    {
        //_sawmill.Debug("TRY ANCHOR TO HARDPOINT RAN");
        var gridUid = Transform(uid).GridUid;
        if (gridUid is null)
            return false;
        if (!TryComp<MapGridComponent>(gridUid, out var gridComp))
            return false;
        if (!_transformSystem.TryGetGridTilePosition(uid, out var indice, gridComp))
        {
            return false;
        }

        foreach (var entity in _mapSystem.GetAnchoredEntities(new Entity<MapGridComponent>(gridUid.Value, gridComp), indice))
        {
            if (!TryComp<HardpointComponent>(entity, out var hardComp))
                continue;
            if (hardComp.anchoring is not null)
                continue;
            if ((hardComp.CompatibleTypes & component.CompatibleTypes) == 0)
                continue;
            if (hardComp.CompatibleSizes < component.CompatibleSizes)
                continue;
            AnchorEntityToHardpoint(uid, entity, component, hardComp, gridUid.Value);
            return true;
        }

        return false;
    }

    public void AnchorEntityToHardpoint(EntityUid target, EntityUid anchor, HardpointAnchorableOnlyComponent targetComp, HardpointComponent hardpoint, EntityUid grid)
    {
        //_sawmill.Debug("ANCHOR ENTITY TO HARDPOINT RAN");
        hardpoint.anchoring = target;
        targetComp.anchoredTo = anchor;
        _transformSystem.SetLocalRotation(target, Transform(anchor).LocalRotation);
        HardpointCannonAnchoredEvent arg = new();
        arg.cannonUid = target;
        arg.gridUid = grid;
        RaiseLocalEvent(anchor, arg);
        DirtyEntity(target);
        DirtyEntity(anchor);
        //Dirty(target, targetComp);
    }
}
