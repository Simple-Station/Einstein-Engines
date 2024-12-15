using System.Linq;
using Content.Server.Power.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.PowerCell.Components;
using Content.Shared.Silicon;
using Content.Shared.Verbs;
using Robust.Shared.Utility;
using Content.Server.Silicon.Charge;
using Content.Server.Power.EntitySystems;
using Content.Server.Popups;
using Content.Server.PowerCell;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server.Power;

public sealed class BatteryDrinkerSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly SiliconChargeSystem _silicon = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryComponent, GetVerbsEvent<AlternativeVerb>>(AddAltVerb);

        SubscribeLocalEvent<BatteryDrinkerComponent, BatteryDrinkerDoAfterEvent>(OnDoAfter);
    }

    private void AddAltVerb(EntityUid uid, BatteryComponent batteryComponent, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract
            || !TryComp<BatteryDrinkerComponent>(args.User, out var drinkerComp)
            || !TestDrinkableBattery(uid, drinkerComp)
            || !_silicon.TryGetSiliconBattery(args.User, out var _))
            return;

        AlternativeVerb verb = new()
        {
            Act = () => DrinkBattery(uid, args.User, drinkerComp),
            Text = Loc.GetString("battery-drinker-verb-drink"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/smite.svg.192dpi.png")),
        };

        args.Verbs.Add(verb);
    }

    private bool TestDrinkableBattery(EntityUid target, BatteryDrinkerComponent drinkerComp)
    {
        if (!drinkerComp.DrinkAll && !HasComp<BatteryDrinkerSourceComponent>(target))
            return false;

        return true;
    }

    private void DrinkBattery(EntityUid target, EntityUid user, BatteryDrinkerComponent drinkerComp)
    {
        var doAfterTime = drinkerComp.DrinkSpeed;

        if (TryComp<BatteryDrinkerSourceComponent>(target, out var sourceComp))
            doAfterTime *= sourceComp.DrinkSpeedMulti;
        else
            doAfterTime *= drinkerComp.DrinkAllMultiplier;

        var args = new DoAfterArgs(EntityManager, user, doAfterTime, new BatteryDrinkerDoAfterEvent(), user, target) // TODO: Make this doafter loop, once we merge Upstream.
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            Broadcast = false,
            DistanceThreshold = 1.35f,
            RequireCanInteract = true,
            CancelDuplicate = false
        };

        _doAfter.TryStartDoAfter(args);
    }

    private void OnDoAfter(EntityUid uid, BatteryDrinkerComponent drinkerComp, DoAfterEvent args)
    {
        if (args.Cancelled || args.Target == null
            || !TryComp<BatteryComponent>(args.Target.Value, out var sourceBattery)
            || !_silicon.TryGetSiliconBattery(uid, out var drinkerBatteryComponent)
            || !TryComp(uid, out PowerCellSlotComponent? batterySlot)
            || !TryComp<BatteryDrinkerSourceComponent>(args.Target.Value, out var sourceComp)
            || !_container.TryGetContainer(uid, batterySlot.CellSlotId, out var container)
            || container.ContainedEntities is null)
            return;

        var source = args.Target.Value;
        var drinkerBattery = container.ContainedEntities.First();
        var amountToDrink = drinkerComp.DrinkMultiplier * 1000;

        amountToDrink = MathF.Min(amountToDrink, sourceBattery.CurrentCharge);
        amountToDrink = MathF.Min(amountToDrink, drinkerBatteryComponent!.MaxCharge - drinkerBatteryComponent.CurrentCharge);

        if (sourceComp.MaxAmount > 0)
            amountToDrink = MathF.Min(amountToDrink, (float) sourceComp.MaxAmount);

        if (amountToDrink <= 0)
        {
            _popup.PopupEntity(Loc.GetString("battery-drinker-empty", ("target", source)), uid, uid);
            return;
        }

        if (_battery.TryUseCharge(source, amountToDrink))
            _battery.SetCharge(drinkerBattery, drinkerBatteryComponent.CurrentCharge + amountToDrink, drinkerBatteryComponent);
        else
        {
            _battery.SetCharge(drinkerBattery, sourceBattery.CurrentCharge + drinkerBatteryComponent.CurrentCharge, drinkerBatteryComponent);
            _battery.SetCharge(source, 0);
        }

        if (sourceComp.DrinkSound is null)
            return;

        _popup.PopupEntity(Loc.GetString("ipc-recharge-tip"), uid, uid, PopupType.SmallCaution);
        _audio.PlayPvs(sourceComp.DrinkSound, source);
        Spawn("EffectSparks", Transform(source).Coordinates);
    }
}
