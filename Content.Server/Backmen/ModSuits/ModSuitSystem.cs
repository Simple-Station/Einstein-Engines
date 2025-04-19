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
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Backmen.ModSuits;
using Content.Shared.Backmen.ModSuits.Components;
using Content.Shared.Clothing;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Mind;
using Content.Shared.Tools.Systems;
using Content.Shared.Whitelist;
using Content.Shared.Wires;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Server.Backmen.ModSuits;

public sealed class ModSuitSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;

    [Dependency] private readonly SharedToolSystem _tool = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

    [Dependency] private readonly SharedMindSystem _mindSystem = default!;

    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    [Dependency] private readonly ClothingSpeedModifierSystem _speedModifier = default!;
    [Dependency] private readonly BatterySystem _battery = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Holy fuck you might say.

        SubscribeLocalEvent<ModSuitComponent, ModSuitUiMessage>(OnToggleClothingMessage);
        SubscribeLocalEvent<ModSuitComponent, TogglePartModulesUiMessage>(OnTogglePartModulesMessage);
        SubscribeLocalEvent<ModSuitComponent, ToggleModuleUiMessage>(OnToggleModuleMessage);

        SubscribeLocalEvent<ModSuitComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<ModSuitComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<ModSuitComponent, ComponentRemove>(OnRemoveModSuit);

        SubscribeLocalEvent<ModSuitComponent, TogglePartDoAfterEvent>(OnDoAfterComplete);
        SubscribeLocalEvent<ModSuitComponent, ToggleModuleDoAfterEvent>(OnModuleDoAfterComplete);
        SubscribeLocalEvent<ModSuitComponent, TogglePartModulesDoAfterEvent>(OnPartModulesDoAfterComplete);

        SubscribeLocalEvent<ModSuitComponent, ToggleModEvent>(OnToggleClothingAction);
        SubscribeLocalEvent<ModSuitComponent, ActivateModEvent>(OnActivateModAction);

        SubscribeLocalEvent<ModSuitComponent, InteractUsingEvent>(OnInteractUsing);

        SubscribeLocalEvent<ModSuitComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<ModSuitComponent, EntRemovedFromContainerMessage>(OnItemRemoved);

        SubscribeLocalEvent<ModSuitComponent, ItemSlotInsertAttemptEvent>(OnModInsertAttempt);
        SubscribeLocalEvent<ModSuitComponent, PanelChangedEvent>(OnPanelToggled);

        SubscribeLocalEvent<ModAttachedClothingComponent, ComponentInit>(OnAttachedInit);
        SubscribeLocalEvent<ModAttachedClothingComponent, GotUnequippedEvent>(OnAttachedUnequip);

        SubscribeLocalEvent<ModAttachedClothingComponent, GetVerbsEvent<EquipmentVerb>>(OnGetAttachedStripVerbsEvent);

        SubscribeLocalEvent<ModSuitComponent, InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>>>(GetRelayedVerbs);
        SubscribeLocalEvent<ModSuitComponent, GetVerbsEvent<EquipmentVerb>>(OnGetVerbs);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ModSuitComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var batteryEnt = comp.BatterySlot.ContainerSlot!.ContainedEntity;
            if (batteryEnt == null)
                continue;

            if (_timing.CurTime < comp.NextBatteryUpdate)
                continue;
            comp.NextBatteryUpdate = _timing.CurTime + comp.BatteryUpdateTime;

            var modConsumptionMultiplier = 1f;
            switch (GetAttachedToggleStatus(uid, comp))
            {
                case ModSuitAttachedStatus.NoneToggled:
                    modConsumptionMultiplier = 0f;
                    break;

                case ModSuitAttachedStatus.PartlyToggled:
                    var pieceDifference = comp.ClothingUids.Count - comp.Container.Count;
                    modConsumptionMultiplier = 1 / (float) comp.ClothingUids.Count * pieceDifference;

                    break;
            }

            var chargeToConsume = comp.PassiveEnergyConsumption * modConsumptionMultiplier;
            foreach (var modComp in comp.ModuleSlots
                             // John Linq!!! Hit the line!!!
                         .Select(moduleSlot => moduleSlot.ContainerSlot!.ContainedEntity)
                         .OfType<EntityUid>()
                         .Select(Comp<ModSuitModComponent>))
            {
                if (modComp.Innate)
                    continue;

                if (modComp.Toggled)
                {
                    chargeToConsume += modComp.ActiveEnergyConsumption;
                }
                else
                {
                    chargeToConsume += modComp.PassiveEnergyConsumption;
                }
            }

            _battery.UseCharge(batteryEnt.Value, chargeToConsume);

            if (Comp<BatteryComponent>(batteryEnt.Value).CurrentCharge <= 0)
            {
                ShutdownAllModules((uid, comp));
            }

            UpdateModUi((uid, comp));
        }
    }

    #region Event Handling

    private void OnInteractUsing(Entity<ModSuitComponent> modSuit, ref InteractUsingEvent args)
    {
        // TODO: Make this be redirected to others too
        if (!_tool.HasQuality(args.Used, "Prying"))
            return;

        var toggledPieces =
            (from modPiece in modSuit.Comp.ClothingUids where !modSuit.Comp.Container.Contains(modPiece.Key) select modPiece.Key).ToList();

        if (toggledPieces.Count <= 0)
            return;

        var clothing = toggledPieces.FirstOrDefault();
        modSuit.Comp.BeingDeployed = true;

        if (modSuit.Comp.ClothingUids.TryGetValue(clothing, out var attachedSlot))
        {
            modSuit.Comp.EntitiesToDeploy.TryAdd(attachedSlot, clothing);
        }

        _popupSystem.PopupEntity(
            Loc.GetString("mod-suit-prying", ("user", args.User)),
            modSuit.Owner,
            PopupType.MediumCaution);

        var comp = modSuit.Comp;
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, comp.ModPartToggleDelay * 2.4, new TogglePartDoAfterEvent(true), modSuit.Owner, clothing, args.Used)
        {
            BreakOnWeightlessMove = false,
        });
    }

    private void OnItemInserted(Entity<ModSuitComponent> modSuit, ref EntInsertedIntoContainerMessage args)
    {
        var inserted = args.Entity;
        if (TryComp<ModSuitModComponent>(inserted, out var modComp))
        {
            if (modComp.Innate)
                return;

            modSuit.Comp.CurrentComplexity += modComp.ModComplexity;
            AddModuleSlot(modSuit);

            if (TryComp<ClothingSpeedModifierComponent>(inserted, out var modify))
            {
                _speedModifier.ModifySpeed(inserted, modify, modComp.SpeedMod);
            }
        }

        UpdateModUi(modSuit);
    }

    private void OnModInsertAttempt(Entity<ModSuitComponent> modSuit, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.User == null)
            return;

        var inserted = args.Item;
        if (!TryComp<ModSuitModComponent>(inserted, out var modComp))
            return;

        foreach (var slot in modSuit.Comp.ModuleSlots)
        {
            // Mod already present
            if (slot.ContainerSlot!.ContainedEntity != null &&
                MetaData(slot.ContainerSlot.ContainedEntity.Value).EntityPrototype!.ID ==
                MetaData(inserted).EntityPrototype!.ID)
            {
                _popupSystem.PopupEntity(
                    Loc.GetString("mod-already-present", ("mod", inserted)),
                    modSuit.Owner,
                    args.User.Value);

                args.Cancelled = true;
                return;
            }

            foreach (var slotAgain in modSuit.Comp.ModuleSlots)
            {
                if (slotAgain.ContainerSlot!.ContainedEntity == null
                    || !modComp.IncompatibleMods.Contains(MetaData(slotAgain.ContainerSlot.ContainedEntity.Value)
                        .EntityPrototype!.ID))
                    continue;

                _popupSystem.PopupEntity(
                    Loc.GetString("mod-incompatible", ("mod", inserted), ("conflict", slotAgain.ContainerSlot!.ContainedEntity)),
                    modSuit.Owner,
                    args.User.Value);

                args.Cancelled = true;

                return;
            }
        }

        if (modSuit.Comp.CurrentComplexity + modComp.ModComplexity > modSuit.Comp.MaxComplexity)
        {
            _popupSystem.PopupEntity(
                Loc.GetString("modsuit-too-complex"),
                modSuit.Owner,
                args.User!.Value);

            args.Cancelled = true;

            return;
        }

        // The panel has to be opened so you can insert mods
        if (!TryComp<WiresPanelComponent>(modSuit, out var panel) || !panel.Open)
        {
            _popupSystem.PopupEntity(
                Loc.GetString("modsuit-open-panel"),
                modSuit.Owner,
                args.User!.Value);

            args.Cancelled = true;
        }
    }

    private void OnItemRemoved(Entity<ModSuitComponent> modSuit, ref EntRemovedFromContainerMessage args)
    {
        var removed = args.Entity;
        if (TryComp<ModSuitModComponent>(removed, out var modComp))
        {
            modSuit.Comp.CurrentComplexity -= modComp.ModComplexity;

            _itemSlotsSystem.RemoveItemSlot(modSuit, modSuit.Comp.ModuleSlots.Last());
            modSuit.Comp.ModuleSlots.Remove(modSuit.Comp.ModuleSlots.Last());

            if (TryComp<ClothingSpeedModifierComponent>(removed, out var modify))
            {
                _speedModifier.ModifySpeed(removed, modify, -modComp.SpeedMod);
            }

            // Is the module currently active?
            if (modComp.Toggled)
                ToggleModule(modSuit, (removed, modComp));
        }

        if (HasComp<BatteryComponent>(removed))
        {
            ShutdownAllModules(modSuit);
        }

        UpdateModUi(modSuit);
    }

    private void OnTogglePartModulesMessage(Entity<ModSuitComponent> modSuit, ref TogglePartModulesUiMessage args)
    {
        var modPart = GetEntity(args.AttachedClothingUid);

        StartPartModulesDoAfter(args.Actor, modSuit, modPart);
    }

    private void OnToggleModuleMessage(Entity<ModSuitComponent> modSuit, ref ToggleModuleUiMessage args)
    {
        var modEnt = GetEntity(args.ModuleUid);
        if (!TryComp<ModSuitModComponent>(modEnt, out var modComp))
            return;

        if (!CanToggleModule(modSuit, (modEnt, modComp), args.Actor))
            return;

        StartModuleDoAfter(args.Actor, modSuit, modEnt);
    }

    private void OnPanelToggled(Entity<ModSuitComponent> modSuit, ref PanelChangedEvent args)
    {
        _itemSlotsSystem.SetLock(modSuit, modSuit.Comp.BatterySlot, !args.Open);

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

        ItemSlot batterySlot = new()
        {
            Whitelist = new EntityWhitelist
            {
                Components = new [] { "Battery" },
            },
            Swap = true,
        };

        _itemSlotsSystem.AddItemSlot(modSuit, comp.BatterySlotId, batterySlot);

        comp.BatterySlot = batterySlot;
        comp.Container = _containerSystem.EnsureContainer<Container>(modSuit, comp.ContainerId);

        _itemSlotsSystem.SetLock(modSuit, comp.BatterySlot, true);

        if (comp.ClothingUids.Count != 0 && comp.DeployActionEntity != null)
            return;

        var xform = Transform(modSuit.Owner);
        var prototypes = comp.ClothingPrototypes;

        foreach (var prototype in prototypes)
        {
            var spawned = Spawn(prototype.Value, xform.Coordinates);
            var attachedClothing = EnsureComp<ModAttachedClothingComponent>(spawned);

            attachedClothing.Slot = prototype.Key;
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

            ToggleModule(modSuit, (moduleEnt, moduleComp));
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
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || comp.ClothingUids.Count == 0)
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
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, comp.ModPartToggleDelay, new TogglePartDoAfterEvent(false), modSuit, clothing, modSuit)
        {
            BreakOnWeightlessMove = false,
        });
    }

    private void StartModuleDoAfter(EntityUid user, Entity<ModSuitComponent> modSuit, EntityUid module)
    {
        modSuit.Comp.BeingDeployed = true;

        var comp = modSuit.Comp;
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, comp.ModuleToggleDelay, new ToggleModuleDoAfterEvent(), modSuit, module, modSuit)
        {
            BreakOnWeightlessMove = false,
        });
    }

    private void StartPartModulesDoAfter(EntityUid user, Entity<ModSuitComponent> modSuit, EntityUid clothing)
    {
        modSuit.Comp.BeingDeployed = true;

        var partModuleCount = 0;
        foreach (var moduleSlot in modSuit.Comp.ModuleSlots)
        {
            if (moduleSlot.ContainerSlot!.ContainedEntity == null)
                continue;

            var modComp = Comp<ModSuitModComponent>(moduleSlot.ContainerSlot!.ContainedEntity.Value);
            if (modComp.Slot == Comp<ModAttachedClothingComponent>(clothing).Slot)
                partModuleCount++;
        }

        var comp = modSuit.Comp;
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(1) + comp.ModuleToggleDelay * partModuleCount, new TogglePartModulesDoAfterEvent(), modSuit, clothing, modSuit)
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

        if (args.Force)
        {
            var attachedComp = Comp<ModAttachedClothingComponent>(args.Target.Value);

            attachedComp.JamTimer =
                _timing.CurTime + _random.Next(attachedComp.JamTimeIntervalMin, attachedComp.JamTimeIntervalMax);
            // yummer
        }

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

    private void OnModuleDoAfterComplete(Entity<ModSuitComponent> modSuit, ref ToggleModuleDoAfterEvent args)
    {
        modSuit.Comp.BeingDeployed = false;
        if (args.Handled || args.Cancelled || args.Target == null)
            return;

        if (!TryComp<ModSuitModComponent>(args.Target, out var modComp))
            return;

        if (!CanToggleModule(modSuit, (args.Target.Value, modComp), args.User))
            return;

        ToggleModule(modSuit, (args.Target.Value, modComp));
    }

    private void OnPartModulesDoAfterComplete(Entity<ModSuitComponent> modSuit, ref TogglePartModulesDoAfterEvent args)
    {
        modSuit.Comp.BeingDeployed = false;
        if (args.Handled || args.Cancelled || args.Target == null)
            return;

        var attachedEnt = args.Target.Value;
        if (!TryComp<ModAttachedClothingComponent>(attachedEnt, out var modPartComp))
            return;

        var modsToToggle = new List<Entity<ModSuitModComponent>>();
        var toggledMods = 0;

        foreach (var module in modSuit.Comp.ModuleSlots)
        {
            if (module.ContainerSlot!.ContainedEntity == null)
                continue;

            var modEnt = module.ContainerSlot!.ContainedEntity.Value;
            if (!TryComp<ModSuitModComponent>(modEnt, out var modComp))
                continue;

            if (!CanToggleModule(modSuit, (modEnt, modComp), args.User))
                continue;

            if (modComp.Slot != modPartComp.Slot)
                continue;

            if (modComp.Toggled)
            {
                toggledMods++;
            }

            modsToToggle.Add((modEnt, modComp));
        }

        if (modsToToggle.Count == toggledMods)
        {
            foreach (var moduleToToggle in modsToToggle)
            {
                ToggleModule(modSuit, moduleToToggle);
            }
        }
        else
        {
            foreach (var moduleToToggle in modsToToggle.Where(moduleToToggle => !moduleToToggle.Comp.Toggled))
            {
                ToggleModule(modSuit, moduleToToggle);
            }
        }
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
            if (modSuit.Comp.ClothingUids
                .Select(modPiece => Comp<ModAttachedClothingComponent>(modPiece.Key))
                .Any(attachedComp => attachedComp.JamTimer > _timing.CurTime))
            {
                _popupSystem.PopupEntity(
                    Loc.GetString("modsuit-jammed"),
                    args.Actor,
                    args.Actor);
                return;
            }

            DeployModSuit(args.Actor, modSuit);
            return;
        }

        var attachedUid = GetEntity(args.AttachedClothingUid);
        StartDoAfter(args.Actor, modSuit, attachedUid);
    }

    private void ShutdownAllModules(Entity<ModSuitComponent> modSuit)
    {
        foreach (var moduleSlot in modSuit.Comp.ModuleSlots)
        {
            var modEnt = moduleSlot.ContainerSlot!.ContainedEntity;
            if (modEnt == null)
                continue;

            var modComp = Comp<ModSuitModComponent>(modEnt.Value);
            if (modComp is { Toggled: true, CanBeToggled: true })
            {
                ToggleModule(modSuit, (modEnt.Value, modComp));
            }
        }
    }

    private bool CanToggleModule(Entity<ModSuitComponent> modSuit, Entity<ModSuitModComponent> module, EntityUid? user)
    {
        if (modSuit.Comp.BeingDeployed)
            return false;

        EntityUid? attachedEnt = null;
        foreach (var clothingPiece in modSuit.Comp.ClothingUids.Where(clothingPiece => clothingPiece.Value == module.Comp.Slot))
        {
            attachedEnt = clothingPiece.Key;
        }

        if (attachedEnt == null)
            return false;

        // The clothing piece is not deployed
        if (modSuit.Comp.Container.Contains(attachedEnt.Value) && user != null)
        {
            _popupSystem.PopupEntity(
                Loc.GetString("modsuit-put-on-first", ("clothing", attachedEnt.Value)),
                modSuit,
                user.Value);
            return false;
        }

        var batteryEnt = modSuit.Comp.BatterySlot.ContainerSlot!.ContainedEntity;
        if (!TryComp<BatteryComponent>(batteryEnt, out var battery))
            return false;

        return battery.CurrentCharge > module.Comp.ActiveEnergyConsumption;
    }

    private void UpdateModUi(Entity<ModSuitComponent> modSuit)
    {
        _appearance.SetData(modSuit,
            ModSuitVisualizerKeys.ClothingPieces,
            new ModSuitVisualizerGroupData(modSuit.Comp.ClothingUids.Keys.Select(id => GetNetEntity(id)).ToList(),
                modSuit.Comp.Container.ContainedEntities.Select(id => GetNetEntity(id)).ToList()));

        var modules = new List<(NetEntity, bool)>();
        foreach (var moduleSlot in modSuit.Comp.ModuleSlots)
        {
            if (moduleSlot.ContainerSlot!.ContainedEntity != null)
            {
                modules.Add(
                    (GetNetEntity(moduleSlot.ContainerSlot!.ContainedEntity.Value),
                    Comp<ModSuitModComponent>(moduleSlot.ContainerSlot!.ContainedEntity.Value).Innate));
            }
        }

        var chargePercent = 0f;
        if (modSuit.Comp.BatterySlot.ContainerSlot!.ContainedEntity != null
            && TryComp<BatteryComponent>(modSuit.Comp.BatterySlot.ContainerSlot!.ContainedEntity, out var battery))
        {
            chargePercent = battery.CurrentCharge / battery.MaxCharge;
        }

        var state = new ModSuitBuiState(chargePercent, modSuit.Comp.CurrentComplexity, modules);
        _uiSystem.SetUiState(modSuit.Owner, ModSuitUiKey.Key, state);
    }

    private void ToggleModule(Entity<ModSuitComponent> modSuit, Entity<ModSuitModComponent> module)
    {
        module.Comp.Toggled = !module.Comp.Toggled;
        var attachedClothings = modSuit.Comp.ClothingUids;

        if (module.Comp.Toggled)
        {
            if (module.Comp.Slot == "MODcore")
            {
                EntityManager.AddComponents(modSuit, module.Comp.Components);
                return;
            }

            foreach (var attached in attachedClothings
                         .Where(attached => attached.Value == module.Comp.Slot))
            {
                EntityManager.AddComponents(attached.Key, module.Comp.Components);
                if (module.Comp.RemoveComponents != null)
                {
                    EntityManager.RemoveComponents(attached.Key, module.Comp.RemoveComponents);
                }

                break;
            }
        }
        else
        {
            if (module.Comp.Slot == "MODcore")
            {
                EntityManager.RemoveComponents(modSuit, module.Comp.Components);
                return;
            }

            foreach (var attached in attachedClothings
                         .Where(attached => attached.Value == module.Comp.Slot))
            {
                EntityManager.RemoveComponents(attached.Key, module.Comp.Components);
                if (module.Comp.RemoveComponents != null)
                {
                    EntityManager.AddComponents(attached.Key, module.Comp.RemoveComponents);
                }

                break;
            }
        }

        UpdateModUi(modSuit);
    }

    /// <summary>
    ///     Equip or unequip the modsuit.
    /// </summary>
    private void OnToggleClothingAction(Entity<ModSuitComponent> modSuit, ref ToggleModEvent args)
    {
        if (args.Handled)
            return;

        // If modsuit have only one attached clothing (like helmets) action will just toggle it
        // If it has more attached clothing elements, it'll open radial menu
        if (GetAttachedToggleStatus(modSuit, modSuit) == ModSuitAttachedStatus.NoneToggled && !modSuit.Comp.BeingDeployed)
        {
            if (modSuit.Comp.ClothingUids
                // Jams are deadly!
                .Select(modPiece => Comp<ModAttachedClothingComponent>(modPiece.Key))
                .Any(attachedComp => attachedComp.JamTimer > _timing.CurTime))
            {
                _popupSystem.PopupEntity(
                    Loc.GetString("modsuit-jammed"),
                    args.Performer,
                    args.Performer);
                return;
            }

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
        if (args.Handled)
            return;

        if (!TryComp<WiresPanelComponent>(modSuit, out var panel) || panel.Open)
        {
            _popupSystem.PopupEntity(
                Loc.GetString("modsuit-close-wires"),
                args.Performer,
                args.Performer);
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

        if (container.Contains(attachedUid))
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

        if (modSuit.Comp.ClothingUids
            // Jams are deadly!
            .Select(modPiece => Comp<ModAttachedClothingComponent>(modPiece.Key))
            .Any(attachedComp => attachedComp.JamTimer > _timing.CurTime))
        {
            _popupSystem.PopupEntity(Loc.GetString("modsuit-jammed"), user, user);
            return;
        }

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
            _popupSystem.PopupEntity(Loc.GetString("modsuit-close-wires"), user, user);
            return;
        }

        EnsureComp<UnremoveableComponent>(modSuit);

        var untoggledPieces =
            modSuit.Comp.ClothingUids
                .Where(piece => modSuit.Comp.Container.Contains(piece.Key))
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
            _popupSystem.PopupEntity(Loc.GetString("modsuit-close-wires"), user, user);
            return;
        }

        var toggledPieces =
            modSuit.Comp.ClothingUids
                .Where(piece => !modSuit.Comp.Container.Contains(piece.Key))
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
            _popupSystem.PopupEntity(Loc.GetString("modsuit-close-wires"), user, user);
            return false;
        }

        if (!TryComp<ModAttachedClothingComponent>(clothing, out var attachedComp))
            return false;

        if (attachedComp.JamTimer > _timing.CurTime)
        {
            _popupSystem.PopupEntity(Loc.GetString("modsuit-clothing-jammed", ("clothing", clothing)), user, user);
            return false;
        }

        if (modSuit.Comp.BatterySlot.ContainerSlot!.ContainedEntity == null
            || !TryComp<BatteryComponent>(modSuit.Comp.BatterySlot.ContainerSlot!.ContainedEntity, out var battery)
            || battery.CurrentCharge <= 0)
        {
            _popupSystem.PopupEntity(Loc.GetString("modsuit-requires-charged-battery"), user, user);
            return false;
        }

        var parent = Transform(modSuit.Owner).ParentUid;
        if (_inventorySystem.TryGetSlotEntity(parent, slot, out var currentClothing))
        {
            if (!modSuit.Comp.ReplaceCurrentClothing)
            {
                _popupSystem.PopupEntity(Loc.GetString("modsuit-remove-first", ("entity", currentClothing)), user, user);
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
        if (modSuit.Comp.ClothingUids.Count == 0)
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
                ? modSuit.Comp.NominalSound
                : modSuit.Comp.ModPiecePutOnSound,
            mind.Session);
    }

    private void UnequipClothing(EntityUid user, Entity<ModSuitComponent> modSuit, EntityUid clothing, string slot)
    {
        var parent = Transform(modSuit.Owner).ParentUid;

        RemComp<UnremoveableComponent>(clothing);
        _inventorySystem.TryUnequip(user, parent, slot, force: true);

        // If attached mod entity has clothing in its container, we equip it back
        if (!TryComp<ModAttachedClothingComponent>(clothing, out var attachedComp) || attachedComp.ClothingContainer == null)
            return;

        var storedClothing = attachedComp.ClothingContainer.ContainedEntity;
        if (storedClothing != null)
            _inventorySystem.TryEquip(parent, storedClothing.Value, slot, force: true);

        if (!_mindSystem.TryGetMind(user, out _, out var mind) || mind.Session == null)
            return;

        _audioSystem.PlayGlobal(modSuit.Comp.ModPiecePutOnSound, mind.Session);
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

        // If the entity doesn't have any attached clothing it means none can be even toggled
        if (attachedClothing.Count == 0)
            return ModSuitAttachedStatus.NoneToggled;

        var toggledCount = attachedClothing.Count(attached => !container.Contains(attached.Key));
        if (toggledCount == 0)
            return ModSuitAttachedStatus.NoneToggled;

        return toggledCount < attachedClothing.Count ? ModSuitAttachedStatus.PartlyToggled : ModSuitAttachedStatus.AllToggled;
    }
}
