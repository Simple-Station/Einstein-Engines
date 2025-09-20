using Content.Shared.Construction;
using Content.Shared.Construction.Components;
using Content.Shared.Construction.EntitySystems;
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
    /// <inheritdoc/>
    public override void Initialize()
    {
        //SubscribeLocalEvent<HardpointAnchorableOnlyComponent, AnchorAttemptEvent>(OnAnchorTry);
        SubscribeLocalEvent<HardpointAnchorableOnlyComponent, AnchorStateChangedEvent>(OnAnchorChange);
        SubscribeLocalEvent<HardpointAnchorableOnlyComponent, MapInitEvent>(OnMapLoad);
        SubscribeLocalEvent<HardpointComponent, AnchorStateChangedEvent>(OnHardpointAnchor);
    }

    public void OnMapLoad(EntityUid uid, HardpointAnchorableOnlyComponent comp, ref MapInitEvent args)
    {
        if (Transform(uid).MapUid == null)
            return;
        if (TryAnchorToAnyHardpoint(uid, comp))
            return;
        Logger.Error(
            $"Hardpoint-only weapon had no hardpoint under itself at mapInit. {uid} , {MetaData(uid).EntityName}");
    }
    public void OnAnchorChange(EntityUid uid, HardpointAnchorableOnlyComponent component, ref AnchorStateChangedEvent args)
    {
        //CONCEPT:
        /*
        "im a shipgun"
        "i just got anchored or deanchored!"
        "if i got anchored, let me check if I've got a valid hardpoint under me."
            "if yes, then set the values properly and stay anchored."
            "if no, then immediately deanchor and send a popup, and a console error message."
        "if i just got deanchored, continue with the deanchoring process that sets the values."
        */
        if (component.anchoredTo is null)
        {
            // Fuck my chungus life just ignore this error. Auto-generated component states can't transmit entity uids properly , SPCR 2025
            //Logger.Error($"SharedHardpointSystem had a anchored entity that wasn't attached to a hardpoint!");
            return;
        }
        var gridUid = Transform(component.anchoredTo.Value).GridUid;
        if (gridUid is null)
            return;

        if (args.Anchored) //the hardpoint just got anchored, so we run the test if we should make it be valid.
        {
            if (TryAnchorToAnyHardpoint(uid, component)) //if it's a valid hardpoint, then we're good
                return;
            else
            {
                Deanchor(uid, component.anchoredTo.Value, gridUid.Value, component); //otherwise, kick that shit out
                _transformSystem.Unanchor(uid);
            }
        }

        Deanchor(uid, component.anchoredTo.Value, gridUid.Value, component);
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
        if (component.anchoredTo is null)
        {
            Logger.Error($"SharedHardpointSystem had a anchored entity that wasn't attached to a hardpoint!");
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
    // public void OnAnchorTry(EntityUid uid, HardpointAnchorableOnlyComponent component, ref AnchorAttemptEvent args)
    // {
    //     if (TryAnchorToAnyHardpoint(uid, component))
    //     {
    //         //AnchorEntityToHardpoint(uid, entity, component, hardComp, gridUid.Value);
    //         return;
    //     }
    //     args.Cancel();
    // }

    /// <summary>
    /// Returns true/false based on if we are able to anchor something here or not.
    /// TRUE: we're good to anchor here, hardpoint fits/exists.
    /// FALSE: there is no hardpoint/it's of the wrong type
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public bool TryAnchorToAnyHardpoint(EntityUid uid, HardpointAnchorableOnlyComponent component)
    {
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
            //instead of doing it here, let's try to use the return true / return false and figure out if we can do it somewhere else
            //and cancel it based on the return true/false
            return true;
        }

        return false;
    }

    public void AnchorEntityToHardpoint(EntityUid target, EntityUid anchor,HardpointAnchorableOnlyComponent targetComp, HardpointComponent hardpoint, EntityUid grid)
    {
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
