using Content.Shared.Actions;
using Content.Shared.Clothing.Components;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Strip;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Shared.Clothing.EntitySystems;

// GOOBSTATION - MODSUITS - THIS SYSTEM FULLY CHANGED
public sealed class ToggleableClothingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedStrippableSystem _strippable = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToggleableClothingComponent, ComponentInit>(OnToggleableInit);
        SubscribeLocalEvent<ToggleableClothingComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ToggleableClothingComponent, ToggleClothingEvent>(OnToggleClothingAction);
        SubscribeLocalEvent<ToggleableClothingComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<ToggleableClothingComponent, ComponentRemove>(OnRemoveToggleable);
        SubscribeLocalEvent<ToggleableClothingComponent, GotUnequippedEvent>(OnToggleableUnequip);
        SubscribeLocalEvent<ToggleableClothingComponent, ToggleableClothingUiMessage>(OnToggleClothingMessage);
        SubscribeLocalEvent<ToggleableClothingComponent, BeingUnequippedAttemptEvent>(OnToggleableUnequipAttempt);

        SubscribeLocalEvent<AttachedClothingComponent, ComponentInit>(OnAttachedInit);
        SubscribeLocalEvent<AttachedClothingComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<AttachedClothingComponent, GotUnequippedEvent>(OnAttachedUnequip);
        SubscribeLocalEvent<AttachedClothingComponent, ComponentRemove>(OnRemoveAttached);
        SubscribeLocalEvent<AttachedClothingComponent, BeingUnequippedAttemptEvent>(OnAttachedUnequipAttempt);

        SubscribeLocalEvent<ToggleableClothingComponent, InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>>>(GetRelayedVerbs);
        SubscribeLocalEvent<ToggleableClothingComponent, GetVerbsEvent<EquipmentVerb>>(OnGetVerbs);
        SubscribeLocalEvent<AttachedClothingComponent, GetVerbsEvent<EquipmentVerb>>(OnGetAttachedStripVerbsEvent);
        SubscribeLocalEvent<ToggleableClothingComponent, ToggleClothingDoAfterEvent>(OnDoAfterComplete);
    }

    private void GetRelayedVerbs(Entity<ToggleableClothingComponent> toggleable, ref InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>> args)
    {
        OnGetVerbs(toggleable, ref args.Args);
    }

    private void OnGetVerbs(Entity<ToggleableClothingComponent> toggleable, ref GetVerbsEvent<EquipmentVerb> args)
    {
        var comp = toggleable.Comp;

        if (!args.CanAccess || !args.CanInteract || args.Hands == null || comp.ClothingUids.Count == 0 || comp.Container == null)
            return;

        var text = comp.VerbText ?? (comp.ActionEntity == null ? null : Name(comp.ActionEntity.Value));
        if (text == null)
            return;

        if (!_inventorySystem.InSlotWithFlags(toggleable.Owner, comp.RequiredFlags))
            return;

        var wearer = Transform(toggleable).ParentUid;
        if (args.User != wearer && comp.StripDelay == null)
            return;

        var user = args.User;

        var verb = new EquipmentVerb()
        {
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/outfit.svg.192dpi.png")),
            Text = Loc.GetString(text),
        };

        if (user == wearer)
        {
            verb.Act = () => ToggleClothing(user, toggleable);
        }
        else
        {
            verb.Act = () => StartDoAfter(user, toggleable, wearer);
        }

        args.Verbs.Add(verb);
    }

    private void StartDoAfter(EntityUid user, Entity<ToggleableClothingComponent> toggleable, EntityUid wearer)
    {
        var comp = toggleable.Comp;

        if (comp.StripDelay == null)
            return;

        var (time, stealth) = _strippable.GetStripTimeModifiers(user, wearer, comp.StripDelay.Value);

        bool hidden = stealth == ThievingStealth.Hidden;

        var args = new DoAfterArgs(EntityManager, user, time, new ToggleClothingDoAfterEvent(), toggleable, wearer, toggleable)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2,
        };

        if (!_doAfter.TryStartDoAfter(args))
            return;

        if (!hidden)
        {
            var popup = Loc.GetString("strippable-component-alert-owner-interact", ("user", Identity.Entity(user, EntityManager)), ("item", toggleable));
            _popupSystem.PopupEntity(popup, wearer, wearer, PopupType.Large);
        }
    }

    private void OnGetAttachedStripVerbsEvent(Entity<AttachedClothingComponent> attached, ref GetVerbsEvent<EquipmentVerb> args)
    {
        var comp = attached.Comp;

        if (!TryComp<ToggleableClothingComponent>(comp.AttachedUid, out var toggleableComp))
            return;

        // redirect to the attached entity.
        OnGetVerbs((comp.AttachedUid, toggleableComp), ref args);
    }

    private void OnDoAfterComplete(Entity<ToggleableClothingComponent> toggleable, ref ToggleClothingDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        ToggleClothing(args.User, toggleable);
    }

    private void OnInteractHand(Entity<AttachedClothingComponent> attached, ref InteractHandEvent args)
    {
        var comp = attached.Comp;

        if (args.Handled)
            return;

        if (!TryComp(comp.AttachedUid, out ToggleableClothingComponent? toggleableComp)
            || toggleableComp.Container == null)
            return;

        // Get slot from dictionary of uid-slot
        if (!toggleableComp.ClothingUids.TryGetValue(attached.Owner, out var attachedSlot))
            return;

        if (!_inventorySystem.TryUnequip(Transform(attached.Owner).ParentUid, attachedSlot, force: true))
            return;

        _containerSystem.Insert(attached.Owner, toggleableComp.Container);
        args.Handled = true;
    }

    /// <summary>
    /// Prevents from unequipping entity if all attached not unequipped
    /// </summary>
    private void OnToggleableUnequipAttempt(Entity<ToggleableClothingComponent> toggleable, ref BeingUnequippedAttemptEvent args)
    {
        var comp = toggleable.Comp;

        if (!comp.BlockUnequipWhenAttached)
            return;

        if (GetAttachedToggleStatus(toggleable) == ToggleableClothingAttachedStatus.NoneToggled)
            return;

        _popupSystem.PopupClient(Loc.GetString("toggleable-clothing-remove-all-attached-first"), args.Unequipee, args.Unequipee);

        args.Cancel();
    }

    /// <summary>
    ///     Called when the suit is unequipped, to ensure that the helmet also gets unequipped.
    /// </summary>
    private void OnToggleableUnequip(Entity<ToggleableClothingComponent> toggleable, ref GotUnequippedEvent args)
    {
        var comp = toggleable.Comp;

        // If it's a part of PVS departure then don't handle it.
        if (_timing.ApplyingState)
            return;

        // Check if container exists and we have linked clothings
        if (comp.Container == null || comp.ClothingUids.Count == 0)
            return;

        var parts = comp.ClothingUids;

        foreach (var part in parts)
        {
            // Check if entity in container what means it already unequipped
            if (comp.Container.Contains(part.Key) || part.Value == null)
                continue;

            _inventorySystem.TryUnequip(args.Equipee, part.Value, force: true);
        }
    }

    private void OnRemoveToggleable(Entity<ToggleableClothingComponent> toggleable, ref ComponentRemove args)
    {
        // If the parent/owner component of the attached clothing is being removed (entity getting deleted?) we will
        // delete the attached entity. We do this regardless of whether or not the attached entity is currently
        // "outside" of the container or not. This means that if a hardsuit takes too much damage, the helmet will also
        // automatically be deleted.

        var comp = toggleable.Comp;

        _actionsSystem.RemoveAction(comp.ActionEntity);

        if (comp.ClothingUids == null || _netMan.IsClient)
            return;

        foreach (var clothing in comp.ClothingUids.Keys)
            QueueDel(clothing);
    }

    private void OnAttachedUnequipAttempt(Entity<AttachedClothingComponent> attached, ref BeingUnequippedAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnRemoveAttached(Entity<AttachedClothingComponent> attached, ref ComponentRemove args)
    {
        // if the attached component is being removed (maybe entity is being deleted?) we will just remove the
        // toggleable clothing component. This means if you had a hard-suit helmet that took too much damage, you would
        // still be left with a suit that was simply missing a helmet. There is currently no way to fix a partially
        // broken suit like this.

        var comp = attached.Comp;

        if (!TryComp(comp.AttachedUid, out ToggleableClothingComponent? toggleableComp)
            || toggleableComp.LifeStage > ComponentLifeStage.Running)
            return;

        var clothingUids = toggleableComp.ClothingUids;

        if (!clothingUids.Remove(attached.Owner) || clothingUids.Count > 0)
            return;

        // If no attached clothing left - remove component and action
        if (clothingUids.Count > 0)
            return;

        _actionsSystem.RemoveAction(toggleableComp.ActionEntity);
        RemComp(comp.AttachedUid, toggleableComp);
    }

    /// <summary>
    ///     Called if the clothing was unequipped, to ensure that it gets moved into the suit's container.
    /// </summary>
    private void OnAttachedUnequip(Entity<AttachedClothingComponent> attached, ref GotUnequippedEvent args)
    {
        var comp = attached.Comp;

        // Death told me to do this- if you need to figure out why each of these are here, idk, figure it out.
        if (_timing.ApplyingState
            || comp.LifeStage > ComponentLifeStage.Running
            || !TryComp(comp.AttachedUid, out ToggleableClothingComponent? toggleableComp)
            || toggleableComp.LifeStage > ComponentLifeStage.Running
            || !toggleableComp.ClothingUids.ContainsKey(attached.Owner))
            return;

        if (toggleableComp.Container != null)
            _containerSystem.Insert(attached.Owner, toggleableComp.Container);
    }

    /// <summary>
    ///     Equip or unequip toggle clothing with ui message
    /// </summary>
    private void OnToggleClothingMessage(Entity<ToggleableClothingComponent> toggleable, ref ToggleableClothingUiMessage args)
    {
        var attachedUid = GetEntity(args.AttachedClothingUid);

        ToggleClothing(args.Actor, toggleable, attachedUid);
    }

    /// <summary>
    ///     Equip or unequip the toggleable clothing.
    /// </summary>
    private void OnToggleClothingAction(Entity<ToggleableClothingComponent> toggleable, ref ToggleClothingEvent args)
    {
        var comp = toggleable.Comp;

        if (args.Handled)
            return;

        if (comp.Container == null || comp.ClothingUids.Count == 0)
            return;

        args.Handled = true;

        // If clothing have only one attached clothing (like helmets) action will just toggle it
        // If it have more attached clothings, it'll open radial menu
        if (comp.ClothingUids.Count == 1)
            ToggleClothing(args.Performer, toggleable, comp.ClothingUids.First().Key);
        else
            _uiSystem.OpenUi(toggleable.Owner, ToggleClothingUiKey.Key, args.Performer);
    }

    /// <summary>
    ///     Toggle function for single clothing
    /// </summary>
    private void ToggleClothing(EntityUid user, Entity<ToggleableClothingComponent> toggleable, EntityUid attachedUid)
    {
        var comp = toggleable.Comp;
        var attachedClothings = comp.ClothingUids;
        var container = comp.Container;

        if (!CanToggleClothing(user, toggleable))
            return;

        if (!attachedClothings.TryGetValue(attachedUid, out var slot) || string.IsNullOrEmpty(slot))
            return;

        if (!container!.Contains(attachedUid))
            UnequipClothing(user, toggleable, attachedUid, slot!);
        else
            EquipClothing(user, toggleable, attachedUid, slot!);
    }

    /// <summary>
    ///     Toggle function for toggling multiple clothings at once
    /// </summary>
    private void ToggleClothing(EntityUid user, Entity<ToggleableClothingComponent> toggleable)
    {
        var comp = toggleable.Comp;
        var attachedClothings = comp.ClothingUids;
        var container = comp.Container;

        if (!CanToggleClothing(user, toggleable))
            return;

        if (GetAttachedToggleStatus(toggleable, comp) == ToggleableClothingAttachedStatus.NoneToggled)
            foreach (var clothing in attachedClothings)
                EquipClothing(user, toggleable, clothing.Key, clothing.Value);
        else
            foreach (var clothing in attachedClothings)
                if (!container!.Contains(clothing.Key))
                    UnequipClothing(user, toggleable, clothing.Key, clothing.Value);
    }

    private bool CanToggleClothing(EntityUid user, Entity<ToggleableClothingComponent> toggleable)
    {
        var comp = toggleable.Comp;
        var attachedClothings = comp.ClothingUids;
        var container = comp.Container;

        if (container == null || attachedClothings.Count == 0)
            return false;

        var ev = new ToggleClothingAttemptEvent(user, toggleable);
        RaiseLocalEvent(toggleable, ev);

        return !ev.Cancelled;
    }

    private void UnequipClothing(EntityUid user, Entity<ToggleableClothingComponent> toggleable, EntityUid clothing, string slot)
    {
        var parent = Transform(toggleable.Owner).ParentUid;

        _inventorySystem.TryUnequip(user, parent, slot, force: true);

        // If attached have clothing in container - equip it
        if (!TryComp<AttachedClothingComponent>(clothing, out var attachedComp) || attachedComp.ClothingContainer == null)
            return;

        var storedClothing = attachedComp.ClothingContainer.ContainedEntity;

        if (storedClothing != null)
            _inventorySystem.TryEquip(parent, storedClothing.Value, slot, force: true);
    }
    private void EquipClothing(EntityUid user, Entity<ToggleableClothingComponent> toggleable, EntityUid clothing, string slot)
    {
        var parent = Transform(toggleable.Owner).ParentUid;
        var comp = toggleable.Comp;

        if (_inventorySystem.TryGetSlotEntity(parent, slot, out var currentClothing))
        {
            // Check if we need to replace current clothing
            if (!TryComp<AttachedClothingComponent>(clothing, out var attachedComp) || !comp.ReplaceCurrentClothing)
            {
                _popupSystem.PopupClient(Loc.GetString("toggleable-clothing-remove-first", ("entity", currentClothing)), user, user);
                return;
            }

            // Check if attached clothing have container or this container not empty
            if (attachedComp.ClothingContainer == null || attachedComp.ClothingContainer.ContainedEntity != null)
                return;

            if (_inventorySystem.TryUnequip(user, parent, slot))
                _containerSystem.Insert(currentClothing.Value, attachedComp.ClothingContainer);
        }

        _inventorySystem.TryEquip(user, parent, clothing, slot);
    }

    private void OnGetActions(Entity<ToggleableClothingComponent> toggleable, ref GetItemActionsEvent args)
    {
        var comp = toggleable.Comp;

        if (comp.ClothingUids.Count == 0 || comp.ActionEntity == null || args.SlotFlags != comp.RequiredFlags)
            return;

        args.AddAction(comp.ActionEntity.Value);
    }

    private void OnToggleableInit(Entity<ToggleableClothingComponent> toggleable, ref ComponentInit args)
    {
        var comp = toggleable.Comp;

        comp.Container = _containerSystem.EnsureContainer<Container>(toggleable, comp.ContainerId);
    }

    private void OnAttachedInit(Entity<AttachedClothingComponent> attached, ref ComponentInit args)
    {
        var comp = attached.Comp;

        comp.ClothingContainer = _containerSystem.EnsureContainer<ContainerSlot>(attached, comp.ClothingContainerId);
    }

    /// <summary>
    ///     On map init, either spawn the appropriate entity into the suit slot, or if it already exists, perform some
    ///     sanity checks. Also updates the action icon to show the toggled-entity.
    /// </summary>
    private void OnMapInit(Entity<ToggleableClothingComponent> toggleable, ref MapInitEvent args)
    {
        var comp = toggleable.Comp;

        if (comp.Container!.Count != 0)
        {
            DebugTools.Assert(comp.ClothingUids.Count != 0, "Unexpected entity present inside of a toggleable clothing container.");
            return;
        }

        if (comp.ClothingUids.Count != 0 && comp.ActionEntity != null)
            return;

        // Add prototype from ClothingPrototype and Slot field to ClothingPrototypes dictionary
        if (comp.ClothingPrototype != null && !string.IsNullOrEmpty(comp.Slot) && !comp.ClothingPrototypes.ContainsKey(comp.Slot))
        {
            comp.ClothingPrototypes.Add(comp.Slot, comp.ClothingPrototype.Value);
        }

        var xform = Transform(toggleable.Owner);

        if (comp.ClothingPrototypes == null)
            return;

        var prototypes = comp.ClothingPrototypes;

        foreach (var prototype in prototypes)
        {
            var spawned = Spawn(prototype.Value, xform.Coordinates);
            var attachedClothing = EnsureComp<AttachedClothingComponent>(spawned);
            attachedClothing.AttachedUid = toggleable;
            EnsureComp<ContainerManagerComponent>(spawned);

            comp.ClothingUids.Add(spawned, prototype.Key);
            _containerSystem.Insert(spawned, comp.Container, containerXform: xform);

            Dirty(spawned, attachedClothing);
        }

        Dirty(toggleable, comp);

        if (_actionContainer.EnsureAction(toggleable, ref comp.ActionEntity, out var action, comp.Action))
            _actionsSystem.SetEntityIcon(comp.ActionEntity.Value, toggleable, action);
    }

    // Checks status of all attached clothings toggle status
    public ToggleableClothingAttachedStatus GetAttachedToggleStatus(EntityUid toggleable, ToggleableClothingComponent? component = null)
    {
        if (!Resolve(toggleable, ref component))
            return ToggleableClothingAttachedStatus.NoneToggled;

        var container = component.Container;
        var attachedClothings = component.ClothingUids;

        // If entity don't have any attached clothings it means none toggled
        if (container == null || attachedClothings.Count == 0)
            return ToggleableClothingAttachedStatus.NoneToggled;

        var toggledCount = attachedClothings.Count(c => !container.Contains(c.Key));

        if (toggledCount == 0)
            return ToggleableClothingAttachedStatus.NoneToggled;

        if (toggledCount < attachedClothings.Count)
            return ToggleableClothingAttachedStatus.PartlyToggled;

        return ToggleableClothingAttachedStatus.AllToggled;
    }

    public List<EntityUid>? GetAttachedClothingsList(EntityUid toggleable, ToggleableClothingComponent? component = null)
    {
        if (!Resolve(toggleable, ref component) || component.ClothingUids.Count == 0)
            return null;

        var newList = new List<EntityUid>();

        foreach (var attachee in component.ClothingUids)
            newList.Add(attachee.Key);

        return newList;
    }
}

public sealed partial class ToggleClothingEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class ToggleClothingDoAfterEvent : SimpleDoAfterEvent
{
}

/// <summary>
///     Event raises on toggleable clothing when someone trying to toggle it
/// </summary>
public sealed class ToggleClothingAttemptEvent : CancellableEntityEventArgs
{
    public EntityUid User { get; }
    public EntityUid Target { get; }

    public ToggleClothingAttemptEvent(EntityUid user, EntityUid target)
    {
        User = user;
        Target = target;
    }
}

/// <summary>
/// Status of toggleable clothing attachee
/// </summary>
[Serializable, NetSerializable]
public enum ToggleableClothingAttachedStatus : byte
{
    NoneToggled,
    PartlyToggled,
    AllToggled
}
