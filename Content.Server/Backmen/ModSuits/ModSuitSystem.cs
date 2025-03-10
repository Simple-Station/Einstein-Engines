using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;
using Content.Shared.Backmen.ModSuits;
using Content.Shared.Backmen.ModSuits.Components;
using Content.Shared.Clothing;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction.Components;
using Content.Shared.Mind;
using Content.Shared.Whitelist;
using Content.Shared.Wires;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Backmen.ModSuits;

public sealed class ModSuitSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly ClothingSpeedModifierSystem _speedModifier = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ModSuitComponent, ModSuitUiMessage>(OnToggleClothingMessage);

        SubscribeLocalEvent<ModSuitComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<ModSuitComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<ModSuitComponent, ComponentRemove>(OnRemoveModSuit);
        SubscribeLocalEvent<ModSuitComponent, TogglePartDoAfterEvent>(OnDoAfterComplete);

        SubscribeLocalEvent<ModSuitComponent, ToggleModEvent>(OnToggleClothingAction);
        SubscribeLocalEvent<ModSuitComponent, ActivateModEvent>(OnActivateModAction);

        SubscribeLocalEvent<ModSuitComponent, EntInsertedIntoContainerMessage>(OnModInserted);
        SubscribeLocalEvent<ModSuitComponent, EntRemovedFromContainerMessage>(OnModRemoved);

        SubscribeLocalEvent<ModSuitComponent, ItemSlotInsertAttemptEvent>(OnModInsertAttempt);
        SubscribeLocalEvent<ModSuitComponent, PanelChangedEvent>(OnPanelToggled);

        SubscribeLocalEvent<ModAttachedClothingComponent, ComponentInit>(OnAttachedInit);
        SubscribeLocalEvent<ModAttachedClothingComponent, GotUnequippedEvent>(OnAttachedUnequip);

        SubscribeLocalEvent<ModAttachedClothingComponent, GetVerbsEvent<EquipmentVerb>>(OnGetAttachedStripVerbsEvent);

        SubscribeLocalEvent<ModSuitComponent, InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>>>(GetRelayedVerbs);
        SubscribeLocalEvent<ModSuitComponent, GetVerbsEvent<EquipmentVerb>>(OnGetVerbs);
    }

    #region Event Handling

    private void OnModInserted(Entity<ModSuitComponent> modSuit, ref EntInsertedIntoContainerMessage args)
    {
        var inserted = args.Entity;
        if (!TryComp<ModSuitModComponent>(inserted, out var modComp))
            return;

        if (!modComp.Innate)
        {
            modSuit.Comp.CurrentComplexity += modComp.ModComplexity;
            AddModuleSlot(modSuit);

            if (TryComp<ClothingSpeedModifierComponent>(inserted, out var modify))
            {
                _speedModifier.ModifySpeed(inserted, modify, modComp.SpeedMod);
            }
        }

        UpdateModUi(modSuit);

        var attachedClothings = modSuit.Comp.ClothingUids;
        if (modComp.Slot == "MODcore")
        {
            EntityManager.AddComponents(inserted, modComp.Components);
            return;
        }

        foreach (var attached in attachedClothings
                     .Where(attached => modSuit.Comp.Container!.Contains(attached.Key))
                     .Where(attached => attached.Value == modComp.Slot))
        {
            EntityManager.AddComponents(attached.Key, modComp.Components);
            if (modComp.RemoveComponents != null)
            {
                EntityManager.RemoveComponents(attached.Key, modComp.RemoveComponents);
            }

            break;
        }
    }

    private void OnModInsertAttempt(Entity<ModSuitComponent> modSuit, ref ItemSlotInsertAttemptEvent args)
    {
        var inserted = args.Item;
        if (!TryComp<ModSuitModComponent>(inserted, out var modComp) || modComp.Innate)
            return;

        foreach (var slot in modSuit.Comp.ModuleSlots)
        {
            // Mod already present
            if (slot.ContainerSlot!.ContainedEntity == null ||
                MetaData(slot.ContainerSlot.ContainedEntity.Value).EntityPrototype!.ID !=
                MetaData(inserted).EntityPrototype!.ID)
                continue;

            _popupSystem.PopupClient(Loc.GetString("mod-already-present", ("mod", inserted)), args.User);
            args.Cancelled = true;

            return;
        }

        if (modSuit.Comp.CurrentComplexity + modComp.ModComplexity > modSuit.Comp.MaxComplexity)
        {
            _popupSystem.PopupClient(Loc.GetString("modsuit-too-complex"), args.User);
            args.Cancelled = true;

            return;
        }

        // The panel has to be opened so you can insert mods
        if (!TryComp<WiresPanelComponent>(modSuit, out var panel) || !panel.Open)
        {
            _popupSystem.PopupClient(Loc.GetString("modsuit-open-panel"), args.User);
            args.Cancelled = true;
        }
    }

    private void OnModRemoved(Entity<ModSuitComponent> modSuit, ref EntRemovedFromContainerMessage args)
    {
        var removed = args.Entity;
        if (!TryComp<ModSuitModComponent>(removed, out var modComp) || modComp.Innate)
            return;

        modSuit.Comp.CurrentComplexity -= modComp.ModComplexity;

        _itemSlotsSystem.RemoveItemSlot(modSuit, modSuit.Comp.ModuleSlots.Last());
        modSuit.Comp.ModuleSlots.Remove(modSuit.Comp.ModuleSlots.Last());

        if (TryComp<ClothingSpeedModifierComponent>(removed, out var modify))
        {
            _speedModifier.ModifySpeed(removed, modify, -modComp.SpeedMod);
        }

        UpdateModUi(modSuit);

        var attachedClothings = modSuit.Comp.ClothingUids;
        if (modComp.Slot == "MODcore")
        {
            EntityManager.RemoveComponents(removed, modComp.Components);
            return;
        }

        foreach (var attached in attachedClothings
                     .Where(attached => modSuit.Comp.Container!.Contains(attached.Key))
                     .Where(attached => attached.Value == modComp.Slot))
        {
            EntityManager.RemoveComponents(attached.Key, modComp.Components);
            if (modComp.RemoveComponents != null)
            {
                EntityManager.AddComponents(attached.Key, modComp.RemoveComponents);
            }

            break;
        }
    }

    private void OnPanelToggled(Entity<ModSuitComponent> modSuit, ref PanelChangedEvent args)
    {
        foreach (var slot in modSuit.Comp.ModuleSlots)
        {
            if (slot.ContainerSlot!.ContainedEntity == null)
            {
                _itemSlotsSystem.SetLock(modSuit, slot, !args.Open);
                continue;
            }

            if (!TryComp<ModSuitModComponent>(slot.ContainerSlot!.ContainedEntity, out var comp) || comp.Innate)
                continue;

            _itemSlotsSystem.SetLock(modSuit, slot, !args.Open);
        }
    }

    /// <summary>
    ///     Fill up the mod suit entity container; Putting inside important stuff like parts of the said mod suit.
    ///     Also updates the action thingy
    /// </summary>
    private void OnInit(Entity<ModSuitComponent> modSuit, ref MapInitEvent args)
    {
        var comp = modSuit.Comp;
        comp.Container = _containerSystem.EnsureContainer<Container>(modSuit, comp.ContainerId);

        if (comp.ClothingUids.Count != 0 && comp.DeployActionEntity != null)
            return;

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

        foreach (var module in modSuit.Comp.InnateModules)
        {
            var moduleEnt = Spawn(module, xform.Coordinates);
            var moduleComp = EnsureComp<ModSuitModComponent>(moduleEnt);

            moduleComp.Innate = true;
            var slot = AddModuleSlot(modSuit);

            _itemSlotsSystem.TryInsert(modSuit.Owner, slot, moduleEnt, modSuit);
            _itemSlotsSystem.SetLock(modSuit, slot, true); // To prevent people from taking out the fucking innate module

            Dirty(moduleEnt, moduleComp);
        }

        // One more slot so we can insert modules into the mod suit, then it's processed by EntInsertedIntoContainerMessage
        AddModuleSlot(modSuit);
        UpdateModUi(modSuit);

        _actionContainer.EnsureAction(modSuit, ref comp.ActivateActionEntity, out _, comp.ActivateAction);
        _actionContainer.EnsureAction(modSuit, ref comp.DeployActionEntity, out _, comp.DeployAction);
    }

    private void OnAttachedInit(Entity<ModAttachedClothingComponent> attached, ref ComponentInit args)
    {
        var comp = attached.Comp;

        comp.ClothingContainer = _containerSystem.EnsureContainer<ContainerSlot>(attached, comp.ClothingContainerId);
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

        var text = comp.VerbText ?? (comp.DeployActionEntity == null ? null : Name(comp.DeployActionEntity.Value));
        if (text == null)
            return;

        if (!_inventorySystem.InSlotWithFlags(modSuit.Owner, comp.RequiredFlags))
            return;

        var wearer = Transform(modSuit).ParentUid;
        if (args.User != wearer)
            return;

        var user = args.User;
        var verb = new EquipmentVerb
        {
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/outfit.svg.192dpi.png")),
            Text = Loc.GetString(text),
            Act = () => ToggleModSuit(user, modSuit),
        };

        args.Verbs.Add(verb);
    }

    private void OnGetAttachedStripVerbsEvent(Entity<ModAttachedClothingComponent> attached, ref GetVerbsEvent<EquipmentVerb> args)
    {
        var comp = attached.Comp;
        if (!TryComp<ModSuitComponent>(comp.AttachedUid, out var modSuitComp))
            return;

        // redirect to the attached entity.
        OnGetVerbs((comp.AttachedUid, modSuitComp), ref args);
    }

    #endregion

    #region Clothing pieces management

    private void StartDoAfter(EntityUid user, Entity<ModSuitComponent> modSuit, EntityUid clothing)
    {
        modSuit.Comp.BeingDeployed = true;
        if (modSuit.Comp.ClothingUids.TryGetValue(clothing, out var attachedSlot))
        {
            modSuit.Comp.EntitiesToDeploy.TryAdd(attachedSlot, clothing);
        }

        var comp = modSuit.Comp;
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, comp.ModPartToggleDelay, new TogglePartDoAfterEvent(), modSuit, clothing, modSuit)
        {
            BreakOnWeightlessMove = false,
        });
    }

    private void OnDoAfterComplete(Entity<ModSuitComponent> modSuit, ref TogglePartDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null)
            return;

        ToggleClothing(args.User, modSuit, args.Target.Value);
        args.Handled = true;

        if (!modSuit.Comp.ClothingUids.TryGetValue(args.Target.Value, out var attachedSlot))
            return;

        if (!modSuit.Comp.EntitiesToDeploy.Remove(attachedSlot))
            return;

        if (modSuit.Comp.EntitiesToDeploy.Count == 0)
        {
            if (GetAttachedToggleStatus(modSuit, modSuit) == ModSuitAttachedStatus.NoneToggled)
            {
                RemComp<UnremoveableComponent>(modSuit);

                _actionsSystem.SetToggled(modSuit.Comp.DeployActionEntity, false);
            }

            modSuit.Comp.BeingDeployed = false;
            return;
        }

        var pieceToToggle = modSuit.Comp.EntitiesToDeploy.First();
        StartDoAfter(args.User, modSuit, pieceToToggle.Value);
    }

    #endregion

    private void OnRemoveModSuit(Entity<ModSuitComponent> modSuit, ref ComponentRemove args)
    {
        var comp = modSuit.Comp;

        _actionsSystem.RemoveAction(comp.DeployActionEntity);
        _actionsSystem.RemoveAction(comp.ActivateActionEntity);

        foreach (var clothing in comp.ClothingUids.Keys)
        {
            QueueDel(clothing);
        }
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
        if (modSuit.Comp.BeingDeployed)
            return;

        if (GetAttachedToggleStatus(modSuit, modSuit) == ModSuitAttachedStatus.NoneToggled)
        {
            DeployModSuit(args.Actor, modSuit);
            return;
        }

        var attachedUid = GetEntity(args.AttachedClothingUid);
        StartDoAfter(args.Actor, modSuit, attachedUid);
    }

    private void UpdateModUi(Entity<ModSuitComponent> modSuit)
    {
        _appearance.SetData(modSuit,
            ModSuitVisualizerKeys.ClothingPieces,
            new ModSuitVisualizerGroupData(modSuit.Comp.ClothingUids.Keys.Select(id => GetNetEntity(id)).ToList(),
                modSuit.Comp.Container!.ContainedEntities.Select(id => GetNetEntity(id)).ToList()));

        var modules = new List<NetEntity>();
        foreach (var moduleSlot in modSuit.Comp.ModuleSlots)
        {
            if (moduleSlot.ContainerSlot!.ContainedEntity != null)
            {
                modules.Add(GetNetEntity(moduleSlot.ContainerSlot!.ContainedEntity.Value));
            }
        }

        var state = new ModSuitBuiState(0f, false, modules);
        _uiSystem.SetUiState(modSuit.Owner, ModSuitUiKey.Key, state);
    }

    /// <summary>
    ///     Equip or unequip the modsuit.
    /// </summary>
    private void OnToggleClothingAction(Entity<ModSuitComponent> modSuit, ref ToggleModEvent args)
    {
        if (args.Handled || modSuit.Comp.BeingDeployed)
            return;

        // If modsuit have only one attached clothing (like helmets) action will just toggle it
        // If it has more attached clothing elements, it'll open radial menu
        if (GetAttachedToggleStatus(modSuit, modSuit) == ModSuitAttachedStatus.NoneToggled)
        {
            DeployModSuit(args.Performer, modSuit);
        }
        else
        {
            _uiSystem.OpenUi(modSuit.Owner, ModSuitDeployUiKey.Key, args.Performer);
        }

        args.Handled = true;
    }

    /// <summary>
    ///     open the panel lol
    /// </summary>
    private void OnActivateModAction(Entity<ModSuitComponent> modSuit, ref ActivateModEvent args)
    {
        if (args.Handled || modSuit.Comp.BeingDeployed)
            return;

        if (!TryComp<WiresPanelComponent>(modSuit, out var panel) || panel.Open)
        {
            _popupSystem.PopupClient(Loc.GetString("modsuit-close-wires"), args.Performer, args.Performer);
            return;
        }

        _uiSystem.OpenUi(modSuit.Owner, ModSuitUiKey.Key, args.Performer);
        args.Handled = true;
    }

    /// <summary>
    ///     Adds a slot for one module inside the mod suit.
    /// </summary>
    /// <param name="modSuit"></param>
    private ItemSlot AddModuleSlot(Entity<ModSuitComponent> modSuit)
    {
        ItemSlot storageComponent = new()
        {
            Whitelist = new EntityWhitelist
            {
                Components = new [] { "ModSuitMod" },
            },
            Swap = false,
        };

        modSuit.Comp.ModuleSlots.Add(storageComponent);
        _itemSlotsSystem.AddItemSlot(
            modSuit.Owner,
            modSuit.Comp.ModuleContainerId + modSuit.Comp.ModuleSlots.Count,
            storageComponent);

        return storageComponent;
    }

    /// <summary>
    ///     Toggle function for single clothing
    /// </summary>
    private void ToggleClothing(EntityUid user, Entity<ModSuitComponent> modSuit, EntityUid attachedUid)
    {
        var comp = modSuit.Comp;
        var attachedClothings = comp.ClothingUids;
        var container = comp.Container;

        if (!attachedClothings.TryGetValue(attachedUid, out var slot) || string.IsNullOrEmpty(slot))
            return;

        if (container!.Contains(attachedUid))
        {
            if (CanEquipClothing(user, modSuit, attachedUid, slot))
            {
                EquipClothing(user, modSuit, attachedUid, slot);
            }
        }
        else
        {
            if (CanUnequipClothing(user, modSuit))
            {
                UnequipClothing(user, modSuit, attachedUid, slot);
            }
        }
    }

    private void ToggleModSuit(EntityUid user, Entity<ModSuitComponent> modSuit)
    {
        if (modSuit.Comp.BeingDeployed)
            return;

        if (GetAttachedToggleStatus(modSuit, modSuit) != ModSuitAttachedStatus.AllToggled)
        {
            DeployModSuit(user, modSuit);
        }
        else
        {
            ObliterateModSuit(user, modSuit);
        }
    }

    private void DeployModSuit(EntityUid user, Entity<ModSuitComponent> modSuit)
    {
        if (!_inventorySystem.InSlotWithFlags(modSuit.Owner, modSuit.Comp.RequiredFlags))
            return;

        if (!TryComp<WiresPanelComponent>(modSuit, out var panel) || panel.Open)
        {
            _popupSystem.PopupClient(Loc.GetString("modsuit-close-wires"), user, user);
            return;
        }

        EnsureComp<UnremoveableComponent>(modSuit);

        var untoggledPieces =
            modSuit.Comp.ClothingUids
                .Where(piece => modSuit.Comp.Container!.Contains(piece.Key))
                .ToDictionary(piece => piece.Value, piece => piece.Key);

        if (untoggledPieces.TryGetValue(modSuit.Comp.FirstSlotToDeploy, out var firstToActivate))
        {
            StartDoAfter(user, modSuit, firstToActivate);
        }
        else
        {
            var pieceToActivate = untoggledPieces.FirstOrDefault();
            StartDoAfter(user, modSuit, pieceToActivate.Value);
        }

        modSuit.Comp.BeingDeployed = true;
        modSuit.Comp.EntitiesToDeploy = untoggledPieces;

        _actionsSystem.SetToggled(modSuit.Comp.DeployActionEntity, true);
    }

    private void ObliterateModSuit(EntityUid user, Entity<ModSuitComponent> modSuit)
    {
        if (!TryComp<WiresPanelComponent>(modSuit, out var panel) || panel.Open)
        {
            _popupSystem.PopupClient(Loc.GetString("modsuit-close-wires"), user, user);
            return;
        }

        var toggledPieces =
            modSuit.Comp.ClothingUids
                .Where(piece => !modSuit.Comp.Container!.Contains(piece.Key))
                .ToDictionary(piece => piece.Value, piece => piece.Key);

        StartDoAfter(user, modSuit, toggledPieces.FirstOrDefault().Value);

        modSuit.Comp.BeingDeployed = true;
        modSuit.Comp.EntitiesToDeploy = toggledPieces;
    }

    private bool CanEquipClothing(EntityUid user, Entity<ModSuitComponent> modSuit, EntityUid clothing, string slot)
    {
        if (!_inventorySystem.InSlotWithFlags(modSuit.Owner, modSuit.Comp.RequiredFlags))
            return false;

        if (!TryComp<WiresPanelComponent>(modSuit, out var panel) || panel.Open)
        {
            _popupSystem.PopupClient(Loc.GetString("modsuit-close-wires"), user, user);
            return false;
        }

        if (!TryComp<ModAttachedClothingComponent>(clothing, out var attachedComp))
            return false;

        var parent = Transform(modSuit.Owner).ParentUid;
        if (_inventorySystem.TryGetSlotEntity(parent, slot, out var currentClothing))
        {
            if (!modSuit.Comp.ReplaceCurrentClothing)
            {
                _popupSystem.PopupClient(Loc.GetString("modsuit-remove-first", ("entity", currentClothing)), user, user);
                return false;
            }

            if (attachedComp.ClothingContainer is not { ContainedEntity: null })
                return false;
        }

        var ev = new EquipModClothingAttemptEvent(user, modSuit);
        RaiseLocalEvent(modSuit, ev);

        return !ev.Cancelled;
    }

    private bool CanUnequipClothing(EntityUid user, Entity<ModSuitComponent> modSuit)
    {
        if (modSuit.Comp.Container == null || modSuit.Comp.ClothingUids.Count == 0)
            return false;

        if (!_inventorySystem.InSlotWithFlags(modSuit.Owner, modSuit.Comp.RequiredFlags))
            return false;

        var ev = new UnequipModClothingAttemptEvent(user, modSuit);
        RaiseLocalEvent(modSuit, ev);

        return !ev.Cancelled;
    }

    private void EquipClothing(EntityUid user, Entity<ModSuitComponent> modSuit, EntityUid clothing, string slot)
    {
        var parent = Transform(modSuit.Owner).ParentUid;
        if (_inventorySystem.TryGetSlotEntity(parent, slot, out var currentClothing))
        {
            var attachedComp = Comp<ModAttachedClothingComponent>(clothing);
            if (_inventorySystem.TryUnequip(user, parent, slot))
                _containerSystem.Insert(currentClothing.Value, attachedComp.ClothingContainer!);
        }

        _inventorySystem.TryEquip(user, parent, clothing, slot, force: true);
        EnsureComp<UnremoveableComponent>(clothing);

        if (!_mindSystem.TryGetMind(user, out _, out var mind) || mind.Session == null)
            return;

        _audioSystem.PlayGlobal(GetAttachedToggleStatus(modSuit, modSuit) == ModSuitAttachedStatus.AllToggled
                ? "/Audio/Backmen/Misc/suit/nominal.ogg"
                : "/Audio/Mecha/mechmove03.ogg",
            mind.Session);
    }

    private void UnequipClothing(EntityUid user, Entity<ModSuitComponent> modSuit, EntityUid clothing, string slot)
    {
        var parent = Transform(modSuit.Owner).ParentUid;

        RemComp<UnremoveableComponent>(clothing);
        _inventorySystem.TryUnequip(user, parent, slot, force: true);

        // If attached have clothing in container - equip it
        if (!TryComp<ModAttachedClothingComponent>(clothing, out var attachedComp) || attachedComp.ClothingContainer == null)
            return;

        var storedClothing = attachedComp.ClothingContainer.ContainedEntity;
        if (storedClothing != null)
            _inventorySystem.TryEquip(parent, storedClothing.Value, slot, force: true);

        if (!_mindSystem.TryGetMind(user, out _, out var mind) || mind.Session == null)
            return;

        _audioSystem.PlayGlobal("/Audio/Mecha/mechmove03.ogg", mind.Session);
    }

    private void OnGetActions(Entity<ModSuitComponent> modSuit, ref GetItemActionsEvent args)
    {
        var comp = modSuit.Comp;
        if (comp.ClothingUids.Count == 0 )
            return;

        if (comp.DeployActionEntity != null)
        {
            args.AddAction(comp.DeployActionEntity.Value);
        }

        if (comp.ActivateActionEntity != null)
        {
            args.AddAction(comp.ActivateActionEntity.Value);
        }
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
