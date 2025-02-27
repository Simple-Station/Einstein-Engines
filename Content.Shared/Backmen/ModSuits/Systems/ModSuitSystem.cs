using Content.Shared.Actions;
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
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;
using Content.Shared.Backmen.ModSuits.Components;
using Content.Shared.Mind;
using Content.Shared.Wires;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.Backmen.ModSuits.Systems;

public sealed class ModSuitSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
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
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ModSuitComponent, ComponentInit>(OnModSuitInit);
        SubscribeLocalEvent<ModSuitComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ModSuitComponent, ToggleModPartEvent>(OnToggleClothingAction);
        SubscribeLocalEvent<ModSuitComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<ModSuitComponent, ComponentRemove>(OnRemoveModSuit);
        SubscribeLocalEvent<ModSuitComponent, GotUnequippedEvent>(OnModSuitUnequip);
        SubscribeLocalEvent<ModSuitComponent, ModSuitUiMessage>(OnToggleClothingMessage);
        SubscribeLocalEvent<ModSuitComponent, BeingUnequippedAttemptEvent>(OnModSuitUnequipAttempt);

        SubscribeLocalEvent<ModAttachedClothingComponent, ComponentInit>(OnAttachedInit);
        SubscribeLocalEvent<ModAttachedClothingComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<ModAttachedClothingComponent, GotUnequippedEvent>(OnAttachedUnequip);
        SubscribeLocalEvent<ModAttachedClothingComponent, ComponentRemove>(OnRemoveAttached);
        SubscribeLocalEvent<ModAttachedClothingComponent, BeingUnequippedAttemptEvent>(OnAttachedUnequipAttempt);

        SubscribeLocalEvent<ModSuitComponent, InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>>>(GetRelayedVerbs);
        SubscribeLocalEvent<ModSuitComponent, GetVerbsEvent<EquipmentVerb>>(OnGetVerbs);
        SubscribeLocalEvent<ModAttachedClothingComponent, GetVerbsEvent<EquipmentVerb>>(OnGetAttachedStripVerbsEvent);
        SubscribeLocalEvent<ModSuitComponent, TogglePartDoAfterEvent>(OnDoAfterComplete);
    }

    private void GetRelayedVerbs(Entity<ModSuitComponent> modSuit, ref InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>> args)
    {
        OnGetVerbs(modSuit, ref args.Args);
    }

    private void OnGetVerbs(Entity<ModSuitComponent> modSuit, ref GetVerbsEvent<EquipmentVerb> args)
    {
        var comp = modSuit.Comp;

        if (!args.CanAccess || !args.CanInteract || args.Hands == null || comp.ClothingUids.Count == 0 || comp.Container == null)
            return;

        var text = comp.VerbText ?? (comp.ActionEntity == null ? null : Name(comp.ActionEntity.Value));
        if (text == null)
            return;

        if (!_inventorySystem.InSlotWithFlags(modSuit.Owner, comp.RequiredFlags))
            return;

        var wearer = Transform(modSuit).ParentUid;
        if (args.User != wearer && comp.StripDelay == null)
            return;

        var user = args.User;

        var verb = new EquipmentVerb
        {
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/outfit.svg.192dpi.png")),
            Text = Loc.GetString(text),
        };

        if (user == wearer)
        {
            verb.Act = () => ToggleClothing(user, modSuit);
        }
        else
        {
            verb.Act = () => StartDoAfter(user, modSuit, wearer);
        }

        args.Verbs.Add(verb);
    }

    private void StartDoAfter(EntityUid user, Entity<ModSuitComponent> modSuit, EntityUid wearer)
    {
        var comp = modSuit.Comp;

        if (comp.StripDelay == null)
            return;

        var (time, stealth) = _strippable.GetStripTimeModifiers(user, wearer, modSuit, comp.StripDelay.Value);

        var args = new DoAfterArgs(EntityManager, user, time, new TogglePartDoAfterEvent(), modSuit, wearer, modSuit)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2,
        };

        if (!_doAfter.TryStartDoAfter(args))
            return;

        if (!stealth)
        {
            var popup = Loc.GetString("strippable-component-alert-owner-interact", ("user", Identity.Entity(user, EntityManager)), ("item", modSuit));
            _popupSystem.PopupEntity(popup, wearer, wearer, PopupType.Large);
        }
    }

    private void OnGetAttachedStripVerbsEvent(Entity<ModAttachedClothingComponent> attached, ref GetVerbsEvent<EquipmentVerb> args)
    {
        var comp = attached.Comp;

        if (!TryComp<ModSuitComponent>(comp.AttachedUid, out var modSuitComp))
            return;

        // redirect to the attached entity.
        OnGetVerbs((comp.AttachedUid, modSuitComp), ref args);
    }

    private void OnDoAfterComplete(Entity<ModSuitComponent> modSuit, ref TogglePartDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        ToggleClothing(args.User, modSuit);
    }

    private void OnInteractHand(Entity<ModAttachedClothingComponent> attached, ref InteractHandEvent args)
    {
        var comp = attached.Comp;
        if (args.Handled)
            return;

        if (!TryComp(comp.AttachedUid, out ModSuitComponent? modSuitComp)
            || modSuitComp.Container == null)
            return;

        // Get slot from dictionary of uid-slot
        if (!modSuitComp.ClothingUids.TryGetValue(attached.Owner, out var attachedSlot))
            return;

        if (!_inventorySystem.TryUnequip(Transform(attached.Owner).ParentUid, attachedSlot, force: true))
            return;

        _containerSystem.Insert(attached.Owner, modSuitComp.Container);
        args.Handled = true;
    }

    /// <summary>
    /// Prevents from unequipping entity if all attached not unequipped
    /// </summary>
    private void OnModSuitUnequipAttempt(Entity<ModSuitComponent> modSuit, ref BeingUnequippedAttemptEvent args)
    {
        var comp = modSuit.Comp;
        if (!comp.BlockUnequipWhenAttached)
            return;

        if (GetAttachedToggleStatus(modSuit) == ModSuitAttachedStatus.NoneToggled)
            return;

        _popupSystem.PopupClient(Loc.GetString("modsuit-remove-all-attached-first"), args.Unequipee, args.Unequipee);

        args.Cancel();
    }

    /// <summary>
    ///     Called when the suit is unequipped, to ensure that the helmet also gets unequipped.
    /// </summary>
    private void OnModSuitUnequip(Entity<ModSuitComponent> modSuit, ref GotUnequippedEvent args)
    {
        var comp = modSuit.Comp;

        // If it's a part of PVS departure then don't handle it.
        if (_timing.ApplyingState)
            return;

        // Check if container exists and we have linked clothings
        if (comp.Container == null || comp.ClothingUids.Count == 0)
            return;

        var parts = comp.ClothingUids;

        foreach (var part in parts.Where(part => !comp.Container.Contains(part.Key)))
        {
            _inventorySystem.TryUnequip(args.Equipee, part.Value, force: true);
        }
    }

    private void OnRemoveModSuit(Entity<ModSuitComponent> modSuit, ref ComponentRemove args)
    {
        // If the parent/owner component of the attached clothing is being removed (entity getting deleted?) we will
        // delete the attached entity. We do this regardless of whether or not the attached entity is currently
        // "outside" of the container or not. This means that if a hardsuit takes too much damage, the helmet will also
        // automatically be deleted.

        var comp = modSuit.Comp;

        _actionsSystem.RemoveAction(comp.ActionEntity);

        if (_netMan.IsClient)
            return;

        foreach (var clothing in comp.ClothingUids.Keys)
        {
            QueueDel(clothing);
        }
    }

    private void OnAttachedUnequipAttempt(Entity<ModAttachedClothingComponent> attached, ref BeingUnequippedAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnRemoveAttached(Entity<ModAttachedClothingComponent> attached, ref ComponentRemove args)
    {
        // if the attached component is being removed (maybe entity is being deleted?) we will just remove the
        // modsuit component. This means if you had a hard-suit helmet that took too much damage, you would
        // still be left with a suit that was simply missing a helmet. There is currently no way to fix a partially
        // broken suit like this.

        var comp = attached.Comp;

        if (!TryComp(comp.AttachedUid, out ModSuitComponent? modSuitComp))
            return;

        if (modSuitComp.LifeStage > ComponentLifeStage.Running)
            return;

        var clothingUids = modSuitComp.ClothingUids;

        if (!clothingUids.Remove(attached.Owner))
            return;

        // If no attached clothing left - remove component and action
        if (clothingUids.Count > 0)
            return;

        _actionsSystem.RemoveAction(modSuitComp.ActionEntity);
        RemComp(comp.AttachedUid, modSuitComp);
    }

    /// <summary>
    ///     Called if the clothing was unequipped, to ensure that it gets moved into the suit's container.
    /// </summary>
    private void OnAttachedUnequip(Entity<ModAttachedClothingComponent> attached, ref GotUnequippedEvent args)
    {
        var comp = attached.Comp;

        // Let containers worry about it.
        if (_timing.ApplyingState)
            return;

        if (comp.LifeStage > ComponentLifeStage.Running)
            return;

        if (!TryComp(comp.AttachedUid, out ModSuitComponent? modSuitComp))
            return;

        if (modSuitComp.LifeStage > ComponentLifeStage.Running)
            return;

        // As unequipped gets called in the middle of container removal, we cannot call a container-insert without causing issues.
        // So we delay it and process it during a system update:
        if (!modSuitComp.ClothingUids.ContainsKey(attached.Owner))
            return;

        if (modSuitComp.Container != null)
            _containerSystem.Insert(attached.Owner, modSuitComp.Container);
    }

    /// <summary>
    ///     Equip or unequip modsuit with ui message
    /// </summary>
    private void OnToggleClothingMessage(Entity<ModSuitComponent> modSuit, ref ModSuitUiMessage args)
    {
        var attachedUid = GetEntity(args.AttachedClothingUid);

        ToggleClothing(args.Actor, modSuit, attachedUid);
    }

    /// <summary>
    ///     Equip or unequip the modsuit.
    /// </summary>
    private void OnToggleClothingAction(Entity<ModSuitComponent> modSuit, ref ToggleModPartEvent args)
    {
        var comp = modSuit.Comp;

        if (args.Handled)
            return;

        if (comp.Container == null || comp.ClothingUids.Count == 0)
            return;

        // If modsuit have only one attached clothing (like helmets) action will just toggle it
        // If it has more attached clothing elements, it'll open radial menu
        if (comp.ClothingUids.Count == 1)
        {
            ToggleClothing(args.Performer, modSuit, comp.ClothingUids.First().Key);
        }
        else
        {
            _uiSystem.OpenUi(modSuit.Owner, ModSuitUiKey.Key, args.Performer);
        }

        args.Handled = true;
    }

    /// <summary>
    ///     Toggle function for single clothing
    /// </summary>
    private void ToggleClothing(EntityUid user, Entity<ModSuitComponent> modSuit, EntityUid attachedUid)
    {
        if (!TryComp<WiresPanelComponent>(modSuit, out var panel) || panel.Open)
        {
            _popupSystem.PopupClient(Loc.GetString("modsuit-close-wires"), user, user);
            return;
        }

        var comp = modSuit.Comp;

        if (!_inventorySystem.InSlotWithFlags(modSuit.Owner, comp.RequiredFlags))
            return;

        var attachedClothings = comp.ClothingUids;
        var container = comp.Container;

        if (!CanToggleClothing(user, modSuit))
            return;

        if (!attachedClothings.TryGetValue(attachedUid, out var slot) || string.IsNullOrEmpty(slot))
            return;

        if (!container!.Contains(attachedUid))
        {
            UnequipClothing(user, modSuit, attachedUid, slot);
        }
        else
        {
            EquipClothing(user, modSuit, attachedUid, slot);
        }
    }

    /// <summary>
    ///     Toggle function for toggling multiple clothings at once
    /// </summary>
    private void ToggleClothing(EntityUid user, Entity<ModSuitComponent> modSuit)
    {
        var comp = modSuit.Comp;
        var attachedClothings = comp.ClothingUids;
        var container = comp.Container;

        if (!CanToggleClothing(user, modSuit))
            return;

        if (GetAttachedToggleStatus(modSuit, comp) == ModSuitAttachedStatus.NoneToggled)
        {
            foreach (var clothing in attachedClothings)
            {
                EquipClothing(user, modSuit, clothing.Key, clothing.Value);
            }
        }
        else
        {
            foreach (var clothing in attachedClothings.Where(clothing => !container!.Contains(clothing.Key)))
            {
                UnequipClothing(user, modSuit, clothing.Key, clothing.Value);
            }
        }
    }

    private bool CanToggleClothing(EntityUid user, Entity<ModSuitComponent> modSuit)
    {
        var comp = modSuit.Comp;
        var attachedClothings = comp.ClothingUids;
        var container = comp.Container;

        if (container == null || attachedClothings.Count == 0)
            return false;

        var ev = new ToggleClothingAttemptEvent(user, modSuit);
        RaiseLocalEvent(modSuit, ev);

        return !ev.Cancelled;
    }

    private void UnequipClothing(EntityUid user, Entity<ModSuitComponent> modSuit, EntityUid clothing, string slot)
    {
        var parent = Transform(modSuit.Owner).ParentUid;

        _inventorySystem.TryUnequip(user, parent, slot, force: true);

        // If attached have clothing in container - equip it
        if (!TryComp<ModAttachedClothingComponent>(clothing, out var attachedComp) || attachedComp.ClothingContainer == null)
            return;

        var storedClothing = attachedComp.ClothingContainer.ContainedEntity;

        if (storedClothing != null)
            _inventorySystem.TryEquip(parent, storedClothing.Value, slot, force: true);
    }
    private void EquipClothing(EntityUid user, Entity<ModSuitComponent> modSuit, EntityUid clothing, string slot)
    {
        var parent = Transform(modSuit.Owner).ParentUid;
        var comp = modSuit.Comp;

        if (_inventorySystem.TryGetSlotEntity(parent, slot, out var currentClothing))
        {
            // Check if we need to replace current clothing
            if (!TryComp<ModAttachedClothingComponent>(clothing, out var attachedComp) || !comp.ReplaceCurrentClothing)
            {
                _popupSystem.PopupClient(Loc.GetString("modsuit-remove-first", ("entity", currentClothing)), user, user);
                return;
            }

            // Check if attached clothing have container or this container not empty
            if (attachedComp.ClothingContainer == null || attachedComp.ClothingContainer.ContainedEntity != null)
                return;

            if (_inventorySystem.TryUnequip(user, parent, slot))
                _containerSystem.Insert(currentClothing.Value, attachedComp.ClothingContainer);
        }

        _inventorySystem.TryEquip(user, parent, clothing, slot, force: true);

        if (GetAttachedToggleStatus(modSuit, modSuit.Comp) != ModSuitAttachedStatus.AllToggled)
            return;

        if (!_mindSystem.TryGetMind(user, out var mindId, out var mind))
            return;

        if (mind.Session == null)
            return;

        // TODO: make it a prototype for the funnies.
        _audioSystem.PlayGlobal("/Audio/ADT/Mecha/nominal.ogg", mind.Session);
    }

    private void OnGetActions(Entity<ModSuitComponent> modSuit, ref GetItemActionsEvent args)
    {
        var comp = modSuit.Comp;
        if (comp.ClothingUids.Count == 0 )
            return;

        if (comp.ActionEntity == null)
            return;

        args.AddAction(comp.ActionEntity.Value);
    }

    private void OnModSuitInit(Entity<ModSuitComponent> modSuit, ref ComponentInit args)
    {
        var comp = modSuit.Comp;

        comp.Container = _containerSystem.EnsureContainer<Container>(modSuit, comp.ContainerId);
    }

    private void OnAttachedInit(Entity<ModAttachedClothingComponent> attached, ref ComponentInit args)
    {
        var comp = attached.Comp;

        comp.ClothingContainer = _containerSystem.EnsureContainer<ContainerSlot>(attached, comp.ClothingContainerId);
    }

    /// <summary>
    ///     On map init, either spawn the appropriate entity into the suit slot, or if it already exists, perform some
    ///     sanity checks. Also updates the action icon to show the toggled-entity.
    /// </summary>
    private void OnMapInit(Entity<ModSuitComponent> modSuit, ref MapInitEvent args)
    {
        var comp = modSuit.Comp;
        if (comp.Container!.Count != 0)
        {
            DebugTools.Assert(comp.ClothingUids.Count != 0, "Unexpected entity present inside of a modsuit container.");
            return;
        }

        if (comp.ClothingUids.Count != 0 && comp.ActionEntity != null)
            return;

        // Add prototype from ClothingPrototype and Slot field to ClothingPrototypes dictionary
        if (comp.ClothingPrototype != null && !string.IsNullOrEmpty(comp.Slot) && !comp.ClothingPrototypes.ContainsKey(comp.Slot))
        {
            comp.ClothingPrototypes.Add(comp.Slot, comp.ClothingPrototype.Value);
        }

        var xform = Transform(modSuit.Owner);
        var prototypes = comp.ClothingPrototypes;

        foreach (var prototype in prototypes)
        {
            var spawned = Spawn(prototype.Value, xform.Coordinates);
            var attachedClothing = EnsureComp<ModAttachedClothingComponent>(spawned);
            attachedClothing.AttachedUid = modSuit;
            EnsureComp<ContainerManagerComponent>(spawned);

            comp.ClothingUids.Add(spawned, prototype.Key);
            _containerSystem.Insert(spawned, comp.Container, containerXform: xform);

            Dirty(spawned, attachedClothing);
        }

        Dirty(modSuit, comp);

        if (_actionContainer.EnsureAction(modSuit, ref comp.ActionEntity, out var action, comp.Action))
            _actionsSystem.SetEntityIcon(comp.ActionEntity.Value, modSuit, action);
    }

    public ModSuitAttachedStatus GetAttachedToggleStatus(EntityUid modSuit, ModSuitComponent? component = null)
    {
        if (!Resolve(modSuit, ref component))
            return ModSuitAttachedStatus.NoneToggled;

        var container = component.Container;
        var attachedClothing = component.ClothingUids;

        // If entity don't have any attached clothings it means none toggled
        if (container == null || attachedClothing.Count == 0)
            return ModSuitAttachedStatus.NoneToggled;

        var toggledCount = attachedClothing.Count(attached => !container.Contains(attached.Key));
        if (toggledCount == 0)
            return ModSuitAttachedStatus.NoneToggled;

        return toggledCount < attachedClothing.Count ? ModSuitAttachedStatus.PartlyToggled : ModSuitAttachedStatus.AllToggled;
    }

    public List<EntityUid>? GetAttachedClothingList(EntityUid modSuit, ModSuitComponent? component = null)
    {
        if (!Resolve(modSuit, ref component) || component.ClothingUids.Count == 0)
            return null;

        return component.ClothingUids.Keys.ToList();
    }
}
