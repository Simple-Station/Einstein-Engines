using System.Linq;
using Content.Shared.Buckle.Components;
using Content.Shared.Construction;
using Content.Shared.Destructible;
using Content.Shared.Foldable;
using Content.Shared.Storage;
using Robust.Shared.Containers;

namespace Content.Shared.Buckle;

public abstract partial class SharedBuckleSystem
{
    private void InitializeStrap()
    {
        SubscribeLocalEvent<StrapComponent, ComponentStartup>(OnStrapStartup);
        SubscribeLocalEvent<StrapComponent, ComponentShutdown>(OnStrapShutdown);
        SubscribeLocalEvent<StrapComponent, ComponentRemove>((_, c, _) => StrapRemoveAll(c));

        SubscribeLocalEvent<StrapComponent, ContainerGettingInsertedAttemptEvent>(OnStrapContainerGettingInsertedAttempt);
<<<<<<< HEAD
        SubscribeLocalEvent<StrapComponent, InteractHandEvent>(OnStrapInteractHand);
        SubscribeLocalEvent<StrapComponent, DestructionEventArgs>((_,c,_) => StrapRemoveAll(c));
        SubscribeLocalEvent<StrapComponent, BreakageEventArgs>((_, c, _) => StrapRemoveAll(c));
=======
        SubscribeLocalEvent<StrapComponent, DestructionEventArgs>((e, c, _) => StrapRemoveAll(e, c));
        SubscribeLocalEvent<StrapComponent, BreakageEventArgs>((e, c, _) => StrapRemoveAll(e, c));
>>>>>>> fa3c89a521 (Partial buckling refactor (#29031))

        SubscribeLocalEvent<StrapComponent, FoldAttemptEvent>(OnAttemptFold);
<<<<<<< HEAD

        SubscribeLocalEvent<StrapComponent, MoveEvent>(OnStrapMoveEvent);
        SubscribeLocalEvent<StrapComponent, MachineDeconstructedEvent>((_, c, _) => StrapRemoveAll(c));
=======
        SubscribeLocalEvent<StrapComponent, MachineDeconstructedEvent>((e, c, _) => StrapRemoveAll(e, c));
>>>>>>> fa3c89a521 (Partial buckling refactor (#29031))
    }

    private void OnStrapStartup(EntityUid uid, StrapComponent component, ComponentStartup args)
    {
        Appearance.SetData(uid, StrapVisuals.State, component.BuckledEntities.Count != 0);
    }

    private void OnStrapShutdown(EntityUid uid, StrapComponent component, ComponentShutdown args)
    {
<<<<<<< HEAD
        if (LifeStage(uid) > EntityLifeStage.MapInitialized)
            return;

        StrapRemoveAll(component);
    }

    private void OnStrapEntModifiedFromContainer(EntityUid uid, StrapComponent component, ContainerModifiedMessage message)
    {
        if (_gameTiming.ApplyingState)
            return;

        foreach (var buckledEntity in component.BuckledEntities)
        {
            if (!TryComp<BuckleComponent>(buckledEntity, out var buckleComp))
            {
                continue;
            }

            ContainerModifiedReAttach(buckledEntity, uid, buckleComp, component);
        }
    }

    private void ContainerModifiedReAttach(EntityUid buckleUid, EntityUid strapUid, BuckleComponent? buckleComp = null, StrapComponent? strapComp = null)
    {
        if (!Resolve(buckleUid, ref buckleComp, false) ||
            !Resolve(strapUid, ref strapComp, false))
            return;

        var contained = _container.TryGetContainingContainer(buckleUid, out var ownContainer);
        var strapContained = _container.TryGetContainingContainer(strapUid, out var strapContainer);

        if (contained != strapContained || ownContainer != strapContainer)
        {
            TryUnbuckle(buckleUid, buckleUid, true, buckleComp);
            return;
        }

        if (!contained)
        {
            ReAttach(buckleUid, strapUid, buckleComp, strapComp);
        }
=======
        if (!TerminatingOrDeleted(uid))
            StrapRemoveAll(uid, component);
>>>>>>> fa3c89a521 (Partial buckling refactor (#29031))
    }

    private void OnStrapContainerGettingInsertedAttempt(EntityUid uid, StrapComponent component, ContainerGettingInsertedAttemptEvent args)
    {
        // If someone is attempting to put this item inside of a backpack, ensure that it has no entities strapped to it.
        if (args.Container.ID == StorageComponent.ContainerId && component.BuckledEntities.Count != 0)
            args.Cancel();
    }

<<<<<<< HEAD
    private void OnStrapInteractHand(EntityUid uid, StrapComponent component, InteractHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = ToggleBuckle(args.User, args.User, uid);
    }

    private void AddStrapVerbs(EntityUid uid, StrapComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (args.Hands == null || !args.CanAccess || !args.CanInteract || !component.Enabled)
            return;

        // Note that for whatever bloody reason, buckle component has its own interaction range. Additionally, this
        // range can be set per-component, so we have to check a modified InRangeUnobstructed for every verb.

        // Add unstrap verbs for every strapped entity.
        foreach (var entity in component.BuckledEntities)
        {
            var buckledComp = Comp<BuckleComponent>(entity);

            if (!_interaction.InRangeUnobstructed(args.User, args.Target, range: buckledComp.Range))
                continue;

            var verb = new InteractionVerb()
            {
                Act = () => TryUnbuckle(entity, args.User, buckleComp: buckledComp),
                Category = VerbCategory.Unbuckle,
                Text = entity == args.User
                    ? Loc.GetString("verb-self-target-pronoun")
                    : Comp<MetaDataComponent>(entity).EntityName
            };

            // In the event that you have more than once entity with the same name strapped to the same object,
            // these two verbs will be identical according to Verb.CompareTo, and only one with actually be added to
            // the verb list. However this should rarely ever be a problem. If it ever is, it could be fixed by
            // appending an integer to verb.Text to distinguish the verbs.

            args.Verbs.Add(verb);
        }

        // Add a verb to buckle the user.
        if (TryComp<BuckleComponent>(args.User, out var buckle) &&
            buckle.BuckledTo != uid &&
            args.User != uid &&
            StrapHasSpace(uid, buckle, component) &&
            _interaction.InRangeUnobstructed(args.User, args.Target, range: buckle.Range))
        {
            InteractionVerb verb = new()
            {
                Act = () => TryBuckle(args.User, args.User, args.Target, buckle),
                Category = VerbCategory.Buckle,
                Text = Loc.GetString("verb-self-target-pronoun")
            };
            args.Verbs.Add(verb);
        }

        // If the user is currently holding/pulling an entity that can be buckled, add a verb for that.
        if (args.Using is {Valid: true} @using &&
            TryComp<BuckleComponent>(@using, out var usingBuckle) &&
            StrapHasSpace(uid, usingBuckle, component) &&
            _interaction.InRangeUnobstructed(@using, args.Target, range: usingBuckle.Range))
        {
            // Check that the entity is unobstructed from the target (ignoring the user).
            bool Ignored(EntityUid entity) => entity == args.User || entity == args.Target || entity == @using;
            if (!_interaction.InRangeUnobstructed(@using, args.Target, usingBuckle.Range, predicate: Ignored))
                return;

            var isPlayer = _playerManager.TryGetSessionByEntity(@using, out var _);
            InteractionVerb verb = new()
            {
                Act = () => TryBuckle(@using, args.User, args.Target, usingBuckle),
                Category = VerbCategory.Buckle,
                Text = Comp<MetaDataComponent>(@using).EntityName,
                // just a held object, the user is probably just trying to sit down.
                // If the used entity is a person being pulled, prioritize this verb. Conversely, if it is
                Priority = isPlayer ? 1 : -1
            };

            args.Verbs.Add(verb);
        }
    }

    private void OnCanDropTarget(EntityUid uid, StrapComponent component, ref CanDropTargetEvent args)
    {
        args.CanDrop = StrapCanDragDropOn(uid, args.User, uid, args.Dragged, component);
        args.Handled = true;
    }

=======
>>>>>>> fa3c89a521 (Partial buckling refactor (#29031))
    private void OnAttemptFold(EntityUid uid, StrapComponent component, ref FoldAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        args.Cancelled = component.BuckledEntities.Count != 0;
    }

<<<<<<< HEAD
    private void OnStrapDragDropTarget(EntityUid uid, StrapComponent component, ref DragDropTargetEvent args)
    {
        if (!StrapCanDragDropOn(uid, args.User, uid, args.Dragged, component))
            return;

        args.Handled = TryBuckle(args.Dragged, args.User, uid);
    }

    private void OnStrapMoveEvent(EntityUid uid, StrapComponent component, ref MoveEvent args)
    {
        // TODO: This looks dirty af.
        // On rotation of a strap, reattach all buckled entities.
        // This fixes buckle offsets and draw depths.
        // This is mega cursed. Please somebody save me from Mr Buckle's wild ride.
        // Oh god I'm back here again. Send help.

        // Consider a chair that has a player strapped to it. Then the client receives a new server state, showing
        // that the player entity has moved elsewhere, and the chair has rotated. If the client applies the player
        // state, then the chairs transform comp state, and then the buckle state. The transform state will
        // forcefully teleport the player back to the chair (client-side only). This causes even more issues if the
        // chair was teleporting in from nullspace after having left PVS.
        //
        // One option is to just never trigger re-buckles during state application.
        // another is to.. just not do this? Like wtf is this code. But I CBF with buckle atm.

        if (_gameTiming.ApplyingState || args.NewRotation == args.OldRotation)
            return;

        foreach (var buckledEntity in component.BuckledEntities)
        {
            if (!TryComp<BuckleComponent>(buckledEntity, out var buckled))
                continue;

            if (!buckled.Buckled || buckled.LastEntityBuckledTo != uid)
            {
                Log.Error($"A moving strap entity {ToPrettyString(uid)} attempted to re-parent an entity that does not 'belong' to it {ToPrettyString(buckledEntity)}");
                continue;
            }

            ReAttach(buckledEntity, uid, buckled, component);
            Dirty(buckled);
        }
    }

    private bool StrapCanDragDropOn(
        EntityUid strapUid,
        EntityUid userUid,
        EntityUid targetUid,
        EntityUid buckleUid,
        StrapComponent? strapComp = null,
        BuckleComponent? buckleComp = null)
    {
        if (!Resolve(strapUid, ref strapComp, false) ||
            !Resolve(buckleUid, ref buckleComp, false))
        {
            return false;
        }

        bool Ignored(EntityUid entity) => entity == userUid || entity == buckleUid || entity == targetUid;

        return _interaction.InRangeUnobstructed(targetUid, buckleUid, buckleComp.Range, predicate: Ignored);
    }

=======
>>>>>>> fa3c89a521 (Partial buckling refactor (#29031))
    /// <summary>
    /// Remove everything attached to the strap
    /// </summary>
    private void StrapRemoveAll(StrapComponent strapComp)
    {
        foreach (var entity in strapComp.BuckledEntities.ToArray())
        {
            TryUnbuckle(entity, entity, true);
        }
<<<<<<< HEAD

        strapComp.BuckledEntities.Clear();
        strapComp.OccupiedSize = 0;
        Dirty(strapComp);
=======
>>>>>>> fa3c89a521 (Partial buckling refactor (#29031))
    }

    private bool StrapHasSpace(EntityUid strapUid, BuckleComponent buckleComp, StrapComponent? strapComp = null)
    {
        if (!Resolve(strapUid, ref strapComp, false))
            return false;

        var avail = strapComp.Size;
        foreach (var buckle in strapComp.BuckledEntities)
        {
            avail -= CompOrNull<BuckleComponent>(buckle)?.Size ?? 0;
        }

        return avail >= buckleComp.Size;
    }

    /// <summary>
    /// Sets the enabled field in the strap component to a value
    /// </summary>
    public void StrapSetEnabled(EntityUid strapUid, bool enabled, StrapComponent? strapComp = null)
    {
        if (!Resolve(strapUid, ref strapComp, false) ||
            strapComp.Enabled == enabled)
            return;

        strapComp.Enabled = enabled;
        Dirty(strapUid, strapComp);

        if (!enabled)
            StrapRemoveAll(strapComp);
    }
}
