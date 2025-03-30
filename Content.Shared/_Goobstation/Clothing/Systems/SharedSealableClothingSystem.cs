using Content.Shared._Goobstation.Clothing.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Clothing;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Popups;
using Content.Shared.PowerCell;
using Content.Shared.Verbs;
using Content.Shared.Wires;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Clothing.Systems;

/// System used for sealable clothing
public abstract class SharedSealableClothingSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainerSystem = default!;
    [Dependency] private readonly ComponentTogglerSystem _componentTogglerSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedPowerCellSystem _powerCellSystem = default!;
    [Dependency] private readonly ToggleableClothingSystem _toggleableSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SealableClothingComponent, ClothingPartSealCompleteEvent>(OnPartSealingComplete);

        SubscribeLocalEvent<SealableClothingControlComponent, ClothingControlSealCompleteEvent>(OnControlSealingComplete);
        SubscribeLocalEvent<SealableClothingControlComponent, ClothingGotEquippedEvent>(OnControlEquip);
        SubscribeLocalEvent<SealableClothingControlComponent, ClothingGotUnequippedEvent>(OnControlUnequip);
        SubscribeLocalEvent<SealableClothingControlComponent, ComponentRemove>(OnControlRemove);
        SubscribeLocalEvent<SealableClothingControlComponent, GetItemActionsEvent>(OnControlGetItemActions);
        SubscribeLocalEvent<SealableClothingControlComponent, GetVerbsEvent<Verb>>(OnEquipmentVerb);
        SubscribeLocalEvent<SealableClothingControlComponent, MapInitEvent>(OnControlMapInit);
        SubscribeLocalEvent<SealableClothingControlComponent, SealClothingDoAfterEvent>(OnSealClothingDoAfter);
        SubscribeLocalEvent<SealableClothingControlComponent, SealClothingEvent>(OnControlSealEvent);
        //SubscribeLocalEvent<SealableClothingControlComponent, StartSealingProcessDoAfterEvent>(OnStartSealingDoAfter);
        SubscribeLocalEvent<SealableClothingControlComponent, ToggleClothingAttemptEvent>(OnToggleClothingAttempt);
    }

    #region Events

    /// Toggles components on part when suit complete sealing process
    private void OnPartSealingComplete(Entity<SealableClothingComponent> part, ref ClothingPartSealCompleteEvent args)
        => _componentTogglerSystem.ToggleComponent(part, args.IsSealed);

    /// Toggles components on control when suit complete sealing process
    private void OnControlSealingComplete(Entity<SealableClothingControlComponent> control, ref ClothingControlSealCompleteEvent args)
        => _componentTogglerSystem.ToggleComponent(control, args.IsSealed);

    /// Add/Remove wearer on clothing equip/unequip
    private void OnControlEquip(Entity<SealableClothingControlComponent> control, ref ClothingGotEquippedEvent args)
    {
        control.Comp.WearerEntity = args.Wearer;
        Dirty(control);
    }

    private void OnControlUnequip(Entity<SealableClothingControlComponent> control, ref ClothingGotUnequippedEvent args)
    {
        control.Comp.WearerEntity = null;
        Dirty(control);
    }

    /// Removes seal action on component remove
    private void OnControlRemove(Entity<SealableClothingControlComponent> control, ref ComponentRemove args)
    {
        var comp = control.Comp;

        _actionsSystem.RemoveAction(comp.SealActionEntity);
    }

    /// Ensures seal action to wearer when it equip the seal control
    private void OnControlGetItemActions(Entity<SealableClothingControlComponent> control, ref GetItemActionsEvent args)
    {
        var (uid, comp) = control;

        if (comp.SealActionEntity == null || args.SlotFlags != comp.RequiredControlSlot)
            return;

        args.AddAction(comp.SealActionEntity.Value);
    }

    /// Adds unsealing verbs to sealing control allowing other users to unseal/seal clothing via stripping
    private void OnEquipmentVerb(Entity<SealableClothingControlComponent> control, ref GetVerbsEvent<Verb> args)
    {
        var (uid, comp) = control;
        var user = args.User;

        if (!args.CanComplexInteract
            // Since sealing control in wearer's container system just won't show verb on args.CanAccess
            || !_interactionSystem.InRangeUnobstructed(user, uid)
            || comp.WearerEntity == null
            || comp.WearerEntity != user
            && _actionBlockerSystem.CanInteract(comp.WearerEntity.Value, null))
            return;

        var verbIcon = comp.IsCurrentlySealed ?
            new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/unlock.svg.192dpi.png")) :
            new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/lock.svg.192dpi.png"));

        var verb = new Verb()
        {
            Icon = verbIcon,
            Priority = 5,
            Text = Loc.GetString(comp.VerbText),
            Act = () => TryStartSealToggleProcess(control, user)
        };

        /* This should make as do after to start unsealing of suit with verb, but, for some reason i couldn't figure out, it ends with doAfter enumerator change exception
         *  Would be nice if some can fix this, yet unsealing will be possible only on incapacitated wearers
        if (args.User == comp.WearerEntity)
        {
            verb.Act = () => TryStartSealToggleProcess(control);
        }
        else
        {
            var doAfterArgs = new DoAfterArgs(EntityManager, args.User, comp.NonWearerSealingTime, new StartSealingProcessDoAfterEvent(), uid)
            {
                RequireCanInteract = true,
                BreakOnMove = true,
                BlockDuplicate = true
            };
            verb.Act = () => _doAfterSystem.TryStartDoAfter(doAfterArgs);
        }*/

        args.Verbs.Add(verb);
    }

    /// Ensure actionEntity on map init
    private void OnControlMapInit(Entity<SealableClothingControlComponent> control, ref MapInitEvent args)
    {
        var (uid, comp) = control;
        _actionContainerSystem.EnsureAction(uid, ref comp.SealActionEntity, comp.SealAction);
    }

    /* This should make as do after to start unsealing of suit with verb, but, for some reason i couldn't figure out, it ends with doAfter enumerator change exception
     * Would be nice if some can fix this, yet unsealing will be possible only on incapacitated wearers
    private void OnStartSealingDoAfter(Entity<SealableClothingControlComponent> control, ref StartSealingProcessDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        TryStartSealToggleProcess(control);
    }*/

    /// Trying to start sealing on action. It'll notify wearer if process already started
    private void OnControlSealEvent(Entity<SealableClothingControlComponent> control, ref SealClothingEvent args)
    {
        var (uid, comp) = control;

        if (!_actionBlockerSystem.CanInteract(args.Performer, null))
            return;

        if (comp.IsInProcess)
        {
            if (comp.IsCurrentlySealed)
            {
                _popupSystem.PopupClient(Loc.GetString(comp.SealedInProcessToggleFailPopup), uid, args.Performer);
                _audioSystem.PlayPredicted(comp.FailSound, uid, args.Performer);
            }
            else
            {
                _popupSystem.PopupClient(Loc.GetString(comp.UnsealedInProcessToggleFailPopup), uid, args.Performer);
                _audioSystem.PlayPredicted(comp.FailSound, uid, args.Performer);
            }

            return;
        }

        TryStartSealToggleProcess(control, args.Performer);
    }

    /// Toggle seal on one part and starts same process on next part
    private void OnSealClothingDoAfter(Entity<SealableClothingControlComponent> control, ref SealClothingDoAfterEvent args)
    {
        var (uid, comp) = control;

        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        var part = args.Target;

        if (!TryComp<SealableClothingComponent>(part, out var sealableComponent))
            return;

        sealableComponent.IsSealed = !comp.IsCurrentlySealed;

        Dirty(part.Value, sealableComponent);

        _audioSystem.PlayPvs(sealableComponent.SealUpSound, uid);

        _appearanceSystem.SetData(part.Value, SealableClothingVisuals.Sealed, sealableComponent.IsSealed);

        var ev = new ClothingPartSealCompleteEvent(sealableComponent.IsSealed);
        RaiseLocalEvent(part.Value, ref ev);

        NextSealProcess(control);
    }

    /// Prevents clothing from toggling if it's sealed or in sealing process
    private void OnToggleClothingAttempt(Entity<SealableClothingControlComponent> control, ref ToggleClothingAttemptEvent args)
    {
        var (uid, comp) = control;

        // Popup if currently sealing
        if (comp.IsInProcess)
        {
            _popupSystem.PopupClient(Loc.GetString(comp.UnsealedInProcessToggleFailPopup), uid, args.User);
            _audioSystem.PlayPredicted(comp.FailSound, uid, args.User);
            args.Cancel();

            return;
        }

        // Popup if sealed, but not in process
        if (comp.IsCurrentlySealed)
        {
            _popupSystem.PopupClient(Loc.GetString(comp.CurrentlySealedToggleFailPopup), uid, args.User);
            _audioSystem.PlayPredicted(comp.FailSound, uid, args.User);
            args.Cancel();

            return;
        }

        return;
    }
    #endregion

    /// Tries to start sealing process
    public bool TryStartSealToggleProcess(Entity<SealableClothingControlComponent> control, EntityUid? user = null)
    {
        var (uid, comp) = control;

        // Prevent sealing/unsealing if modsuit don't have wearer or already started process
        if (comp.WearerEntity == null || comp.IsInProcess)
            return false;

        if (user == null)
            user = comp.WearerEntity;

        var ev = new ClothingSealAttemptEvent(user.Value);
        RaiseLocalEvent(control, ev);

        if (ev.Cancelled)
            return false;

        // All parts required to be toggled to perform sealing
        if (_toggleableSystem.GetAttachedToggleStatus(uid) != ToggleableClothingAttachedStatus.AllToggled)
        {
            _popupSystem.PopupClient(Loc.GetString(comp.ToggleFailedPopup), uid, user);
            _audioSystem.PlayPredicted(comp.FailSound, uid, user);
            return false;
        }

        // Trying to get all clothing to seal
        var sealeableList = _toggleableSystem.GetAttachedClothingsList(uid);
        if (sealeableList == null)
            return false;

        foreach (var sealeable in sealeableList)
        {
            if (!HasComp<SealableClothingComponent>(sealeable))
            {
                _popupSystem.PopupEntity(Loc.GetString(comp.ToggleFailedPopup), uid);
                _audioSystem.PlayPredicted(comp.FailSound, uid, user);

                comp.ProcessQueue.Clear();
                Dirty(control);

                return false;
            }

            comp.ProcessQueue.Enqueue(EntityManager.GetNetEntity(sealeable));
        }

        comp.IsInProcess = true;
        Dirty(control);

        NextSealProcess(control);

        return true;
    }

    /// Recursively seals/unseals all parts of sealable clothing
    private void NextSealProcess(Entity<SealableClothingControlComponent> control)
    {
        var (uid, comp) = control;

        // Finish sealing process
        if (comp.ProcessQueue.Count == 0)
        {
            comp.IsInProcess = false;
            comp.IsCurrentlySealed = !comp.IsCurrentlySealed;

            _audioSystem.PlayEntity(comp.IsCurrentlySealed ? comp.SealCompleteSound : comp.UnsealCompleteSound, comp.WearerEntity!.Value, uid);

            var ev = new ClothingControlSealCompleteEvent(comp.IsCurrentlySealed);
            RaiseLocalEvent(control, ref ev);

            _appearanceSystem.SetData(uid, SealableClothingVisuals.Sealed, comp.IsCurrentlySealed);

            Dirty(control);
            return;
        }

        var processingPart = EntityManager.GetEntity(comp.ProcessQueue.Dequeue());
        Dirty(control);

        if (!TryComp<SealableClothingComponent>(processingPart, out var sealableComponent) || !comp.IsInProcess)
        {
            _popupSystem.PopupClient(Loc.GetString(comp.ToggleFailedPopup), uid, comp.WearerEntity);
            _audioSystem.PlayPredicted(comp.FailSound, uid, comp.WearerEntity);

            NextSealProcess(control);
            return;
        }

        // If part is sealed when control trying to seal - it should just skip this part
        if (sealableComponent.IsSealed != comp.IsCurrentlySealed)
        {
            NextSealProcess(control);
            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, uid, sealableComponent.SealingTime, new SealClothingDoAfterEvent(), uid, target: processingPart, showTo: comp.WearerEntity)
        {
            NeedHand = false,
            RequireCanInteract = false,
        };

        // Checking for client here to skip first process popup spam that happens. Predicted popups don't work here because doafter starts on sealable control, not on player.
        if (!_doAfterSystem.TryStartDoAfter(doAfterArgs) || _netManager.IsClient)
            return;

        if (comp.IsCurrentlySealed)

            _popupSystem.PopupEntity(Loc.GetString(sealableComponent.SealDownPopup,
                ("partName", Identity.Name(processingPart, EntityManager))),
                uid, comp.WearerEntity!.Value);
        else
            _popupSystem.PopupEntity(Loc.GetString(sealableComponent.SealUpPopup,
                ("partName", Identity.Name(processingPart, EntityManager))),
                uid, comp.WearerEntity!.Value);
    }
}

[Serializable, NetSerializable]
public sealed partial class SealClothingDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed partial class StartSealingProcessDoAfterEvent : SimpleDoAfterEvent { }

public sealed partial class SealClothingEvent : InstantActionEvent { }

/// Raises on control when clothing finishes it's sealing or unsealing process
[ByRefEvent]
public readonly record struct ClothingControlSealCompleteEvent(bool IsSealed)
{
    public readonly bool IsSealed = IsSealed;
}

/// Raises on part when clothing finishes it's sealing or unsealing process
[ByRefEvent]
public readonly record struct ClothingPartSealCompleteEvent(bool IsSealed)
{
    public readonly bool IsSealed = IsSealed;
}

public sealed partial class ClothingSealAttemptEvent : CancellableEntityEventArgs
{
    public EntityUid User;

    public ClothingSealAttemptEvent(EntityUid user)
    {
        User = user;
    }
}
