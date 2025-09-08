using System.Numerics;
using Content.Shared._Crescent.SpaceArtillery;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.CombatMode;
using Content.Shared.Examine;
using Content.Shared.Actions;
using Content.Shared.Buckle.Components;
using Content.Shared.Stacks;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Shuttles.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Camera;
using Content.Server.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Power;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;

namespace Content.Server._Crescent.SpaceArtillery;

public sealed partial class SpaceArtillerySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly SharedCombatModeSystem _combat = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly DeviceLinkSystem _deviceLink = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedShuttleSystem _shuttleSystem = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoilSystem = default!;

    private const float DISTANCE = 100;
    private const float BIG_DAMAGE = 2500;
    private const float BIG_DAMGE_KICK = 35;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("SpaceArtillery");
        SubscribeLocalEvent<SpaceArtilleryComponent, SignalReceivedEvent>(OnSignalReceived);
        SubscribeLocalEvent<SpaceArtilleryComponent, BuckledEvent>(OnBuckle);
        SubscribeLocalEvent<SpaceArtilleryComponent, UnbuckledEvent>(onUnbuckle);
        SubscribeLocalEvent<SpaceArtilleryComponent, AttemptShootEvent>(OnShotAttempt);
        SubscribeLocalEvent<SpaceArtilleryComponent, FireActionEvent>(OnFireAction);
        SubscribeLocalEvent<SpaceArtilleryComponent, AmmoShotEvent>(OnShotEvent);
        SubscribeLocalEvent<SpaceArtilleryComponent, OnEmptyGunShotEvent>(OnEmptyShotEvent);
        SubscribeLocalEvent<SpaceArtilleryComponent, PowerChangedEvent>(OnApcChanged);
        SubscribeLocalEvent<SpaceArtilleryComponent, ChargeChangedEvent>(OnBatteryChargeChanged);
        SubscribeLocalEvent<SpaceArtilleryComponent, EntInsertedIntoContainerMessage>(OnCoolantSlotChanged);
        SubscribeLocalEvent<SpaceArtilleryComponent, EntRemovedFromContainerMessage>(OnCoolantSlotChanged);
        SubscribeLocalEvent<SpaceArtilleryComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<SpaceArtilleryComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<SpaceArtilleryComponent, ComponentRemove>(OnComponentRemove);

        SubscribeLocalEvent<ShipWeaponProjectileComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnShotAttempt(Entity<SpaceArtilleryComponent> entity, ref AttemptShootEvent ev)
    {
        if (_battery.TryGetBatteryComponent(entity.Owner, out var battery, out var _) &&
            battery.CurrentCharge < entity.Comp.PowerUseActive)
        {
            OnMalfunction(entity.Owner, entity.Comp);
            ev.Cancelled = true;
            return;
        }

        if (!_battery.TryUseCharge(entity.Owner, entity.Comp.PowerUseActive))
        {
            OnMalfunction(entity.Owner, entity.Comp);
            ev.Cancelled = true;
        }

    }
    private void OnComponentInit(EntityUid uid, SpaceArtilleryComponent component, ComponentInit args)
    {
        if (component.IsCoolantRequiredToFire == true)
            _itemSlotsSystem.AddItemSlot(uid, SpaceArtilleryComponent.CoolantSlotSlotId, component.CoolantSlot);
    }

    private void OnComponentRemove(EntityUid uid, SpaceArtilleryComponent component, ComponentRemove args)
    {
        _itemSlotsSystem.RemoveItemSlot(uid, component.CoolantSlot);
    }

    private void OnExamine(EntityUid uid, SpaceArtilleryComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (component.IsArmed == true)
        {
            args.PushMarkup(Loc.GetString("space-artillery-on-examine-safe"));
        }
        else
        {
            args.PushMarkup(Loc.GetString("space-artillery-on-examine-armed"));
        }

        if (component.IsCoolantRequiredToFire == true)
        {
            args.PushMarkup(Loc.GetString("space-artillery-on-examine-coolant-consumed",
            ("consumed_coolant", component.CoolantConsumed)));

            args.PushMarkup(Loc.GetString("space-artillery-on-examine-coolant-count",
            ("current_coolant", component.CoolantStored), ("max_coolant", component.MaxCoolantStored)));
        }
    }

    private void OnSignalReceived(EntityUid uid, SpaceArtilleryComponent component, ref SignalReceivedEvent args)
    {
        if (component.IsPowered == true || component.IsPowerRequiredForSignal == false)
        {
            if (args.Port == component.SpaceArtilleryFirePort && component.IsArmed == true)
            {
                if (TryComp<BatteryComponent>(uid, out var battery))
                {
                    if (component.IsPowered == true && battery.CurrentCharge >= component.PowerUseActive || component.IsPowerRequiredToFire == false)
                    {
                        if (component.IsCoolantRequiredToFire == true && component.CoolantStored >= component.CoolantConsumed || component.IsCoolantRequiredToFire == false)
                            TryFireArtillery(uid, component);
                        else
                            OnMalfunction(uid, component);
                    }
                    else
                        OnMalfunction(uid, component);
                }
            }
            if (args.Port == component.SpaceArtilleryToggleSafetyPort)
            {
                if (TryComp<CombatModeComponent>(uid, out var combat))
                {
                    if (combat.IsInCombatMode == false)
                    {
                        _combat.SetInCombatMode(uid, true, combat);
                        component.IsArmed = true;

                        if (component.IsCapableOfSendingSignal == true)
                            _deviceLink.SendSignal(uid, component.SpaceArtilleryDetectedSafetyChangePort, true);
                    }
                    else
                    {
                        _combat.SetInCombatMode(uid, false, combat);
                        component.IsArmed = false;

                        if (component.IsCapableOfSendingSignal == true)
                            _deviceLink.SendSignal(uid, component.SpaceArtilleryDetectedSafetyChangePort, true);
                    }
                }
            }
            if (args.Port == component.SpaceArtilleryOnSafetyPort)
            {
                if (TryComp<CombatModeComponent>(uid, out var combat))
                {
                    if (combat.IsInCombatMode == true && component.IsCapableOfSendingSignal == true)
                        _deviceLink.SendSignal(uid, component.SpaceArtilleryDetectedSafetyChangePort, true);

                    _combat.SetInCombatMode(uid, false, combat);
                    component.IsArmed = false;

                }
            }
            if (args.Port == component.SpaceArtilleryOffSafetyPort)
            {
                if (TryComp<CombatModeComponent>(uid, out var combat))
                {
                    if (combat.IsInCombatMode == false && component.IsCapableOfSendingSignal == true)
                        _deviceLink.SendSignal(uid, component.SpaceArtilleryDetectedSafetyChangePort, true);

                    _combat.SetInCombatMode(uid, true, combat);
                    component.IsArmed = true;
                }
            }
        }
        else
            OnMalfunction(uid, component);
    }

    private void OnBuckle(EntityUid uid, SpaceArtilleryComponent component, ref BuckledEvent args)
    {

        // Update actions
        if (TryComp<ActionsComponent>(args.Buckle.Owner, out var actions))
        {
            _actionsSystem.AddAction(
                args.Buckle.Owner,
                ref component.FireActionEntity,
                component.FireAction,
                uid,
                actions);
            }
    }

    private void onUnbuckle(EntityUid uid, SpaceArtilleryComponent component, ref UnbuckledEvent args)
    {

        _actionsSystem.RemoveProvidedActions(args.Buckle.Owner, uid);

    }

    /// <summary>
    /// This fires when the gunner presses the fire action
    /// </summary>
    private void OnFireAction(EntityUid uid, SpaceArtilleryComponent component, FireActionEvent args)
    {
        if ((component.IsPowered || !component.IsPowerRequiredForMount) && component.IsArmed)
        {
            if (TryComp<BatteryComponent>(uid, out var battery))
            {
                if ((component.IsPowered && battery.CurrentCharge >= component.PowerUseActive) || !component.IsPowerRequiredToFire)
                {
                    if (component.IsCoolantRequiredToFire && component.CoolantStored >= component.CoolantConsumed || !component.IsCoolantRequiredToFire)
                    {
                        if (args.Handled)
                            return;

                        TryFireArtillery(uid, component);

                        args.Handled = true;
                    }
                    else
                        OnMalfunction(uid, component);
                }
                else
                    OnMalfunction(uid, component);
            }
        }
        else
            OnMalfunction(uid, component);
    }


    private void OnApcChanged(EntityUid uid, SpaceArtilleryComponent component, ref PowerChangedEvent args)
    {
        if (TryComp<BatterySelfRechargerComponent>(uid, out var batteryCharger))
        {
            if (args.Powered)
            {
                component.IsCharging = true;
                batteryCharger.AutoRecharge = true;
                batteryCharger.AutoRechargeRate = component.PowerChargeRate;
            }
            else
            {
                component.IsCharging = false;
                batteryCharger.AutoRecharge = true;
                batteryCharger.AutoRechargeRate = component.PowerUsePassive * -1;

                if (TryComp<BatteryComponent>(uid, out var battery))
                    _battery.UseCharge(uid, component.PowerUsePassive, battery); //It is done so that BatterySelfRecharger will get start operating instead of being blocked by fully charged battery
            }
        }
    }


    private void OnBatteryChargeChanged(EntityUid uid, SpaceArtilleryComponent component, ref ChargeChangedEvent args)
    {
        if (args.Charge > 0)
        {
            component.IsPowered = true;
        }
        else
        {
            component.IsPowered = false;
        }

        if (TryComp<ApcPowerReceiverComponent>(uid, out var apcPowerReceiver) && TryComp<BatteryComponent>(uid, out var battery))
        {
            if (battery.IsFullyCharged == false)
            {
                apcPowerReceiver.Load = component.PowerUsePassive + component.PowerChargeRate;
            }
            else
            {
                apcPowerReceiver.Load = component.PowerUsePassive;
            }
        }
    }

    private void OnCoolantSlotChanged(EntityUid uid, SpaceArtilleryComponent component, ContainerModifiedMessage args)
    {
        GetInsertedCoolantAmount(component, out var storage);

        // validating the coolant slot was setup correctly in the yaml
        if (component.CoolantSlot.ContainerSlot is not BaseContainer coolantSlot)
        {
            return;
        }

        // validate stack prototypes
        if (!TryComp<StackComponent>(component.CoolantSlot.ContainerSlot.ContainedEntity, out var stackComponent) ||
                stackComponent.StackTypeId == null)
        {
            return;
        }

        // and then check them against the Armament's CoolantType
        if (_prototypeManager.Index<StackPrototype>(component.CoolantType) != _prototypeManager.Index<StackPrototype>(stackComponent.StackTypeId))
            return;

        var currentCoolant = component.CoolantStored;
        var maxCoolant = component.MaxCoolantStored;
        var totalCoolantPresent = currentCoolant + storage;
        if (totalCoolantPresent > maxCoolant)
        {
            var remainingCoolant = totalCoolantPresent - maxCoolant;
            stackComponent.Count = remainingCoolant;
            stackComponent.UiUpdateNeeded = true;
            component.CoolantStored = maxCoolant;
        }
        else
        {
            component.CoolantStored = totalCoolantPresent;
            _containerSystem.CleanContainer(coolantSlot);
        }
    }

    private void TryFireArtillery(EntityUid uid, SpaceArtilleryComponent component)
    {
        var xform = Transform(uid);

        if (!_gun.TryGetGun(uid, out var gunUid, out var gun))
        {
            OnMalfunction(uid, component);
            return;
        }
        var worldPosX = _xform.GetWorldPosition(uid).X;
        var worldPosY = _xform.GetWorldPosition(uid).Y;
        var worldRot = _xform.GetWorldRotation(uid) + Math.PI;
        var targetSpot = new Vector2(worldPosX - DISTANCE * (float) Math.Sin(worldRot), worldPosY + DISTANCE * (float) Math.Cos(worldRot));

        EntityCoordinates targetCordinates;
        targetCordinates = new EntityCoordinates(xform.MapUid!.Value, targetSpot);

        _gun.AttemptShoot(uid, gunUid, gun, targetCordinates);
    }

    private void GetInsertedCoolantAmount(SpaceArtilleryComponent component, out int amount)
    {
        amount = 0;
        var coolantEntity = component.CoolantSlot.ContainerSlot?.ContainedEntity;

        if (!TryComp<StackComponent>(coolantEntity, out var coolantStack) ||
            coolantStack.StackTypeId != component.CoolantType)
        {
            return;
        }

        amount = coolantStack.Count;
        return;
    }

    ///TODO Fix empty cartridge allowing recoil to be activated
    ///TODO add check for args.FiredProjectiles
    private void OnShotEvent(EntityUid uid, SpaceArtilleryComponent component, AmmoShotEvent args)
    {
        if (args.FiredProjectiles.Count == 0)
        {
            OnMalfunction(uid, component);
            return;
        }

        if (TryComp<BatteryComponent>(uid, out var battery))
        {
            var worldPosX = _xform.GetWorldPosition(uid).X;
            var worldPosY = _xform.GetWorldPosition(uid).Y;
            var worldRot = _xform.GetWorldRotation(uid) + Math.PI;
            var targetSpot = new Vector2(worldPosX - DISTANCE * (float) Math.Sin(worldRot), worldPosY + DISTANCE * (float) Math.Cos(worldRot));

            var xformGridUid = Transform(uid).GridUid;

            if (component.IsCapableOfSendingSignal == true)
                _deviceLink.SendSignal(uid, component.SpaceArtilleryDetectedFiringPort, true);

            if (component.IsPowerRequiredToFire == true)
            {
                _battery.UseCharge(uid, component.PowerUseActive, battery);
            }
            if (component.IsCoolantRequiredToFire == true)
            {
                component.CoolantStored -= component.CoolantConsumed;
            }
        }
    }

    private void OnEmptyShotEvent(EntityUid uid, SpaceArtilleryComponent component, OnEmptyGunShotEvent args)
    {
        OnMalfunction(uid, component);
    }

    private void OnMalfunction(EntityUid uid, SpaceArtilleryComponent component)
    {
        if (component.IsCapableOfSendingSignal == true)
            _deviceLink.SendSignal(uid, component.SpaceArtilleryDetectedMalfunctionPort, true);
    }

    private void OnProjectileHit(EntityUid uid, ShipWeaponProjectileComponent component, ProjectileHitEvent hitEvent)
    {

        var grid = Transform(hitEvent.Target).GridUid;
        if (grid == null)
            return;

        var players = Filter.Empty();
        players.AddInGrid((EntityUid) grid);

        foreach (var player in players.Recipients)
        {
            if (player.AttachedEntity is not EntityUid playerEnt)
                continue;

            var vector = _xform.GetWorldPosition(uid) - _xform.GetWorldPosition(playerEnt);

            _recoilSystem.KickCamera(playerEnt, vector.Normalized() * (float) hitEvent.Damage.GetTotal() / BIG_DAMAGE * BIG_DAMGE_KICK);
        }

    }
}
