using System.Numerics;
using Content.Server.Antag.Mimic;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.VendingMachines;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Server.GameObjects;
using Robust.Shared.Physics.Systems;
using System.Linq;
using Robust.Shared.Physics;
using Content.Shared.Movement.Components;
using Content.Shared.Damage;
using Content.Server.NPC.HTN;
using Content.Server.NPC;
using Content.Shared.Weapons.Melee;
using Content.Server.Advertise.EntitySystems;
using Content.Server.Advertise.Components;
using Content.Server.Power.Components;
using Content.Shared.CombatMode;
using Content.Server.Station.Systems;
using Content.Server.GameTicking;
using Content.Server.Chat.Systems;
using Content.Server.NPC.Systems;

namespace Content.Server.Antag;

public sealed class MobReplacementRuleSystem : GameRuleSystem<MobReplacementRuleComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly AdvertiseSystem _advertise = default!;


    protected override void Started(EntityUid uid, MobReplacementRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = AllEntityQuery<VendingMachineComponent, TransformComponent>();
        var spawns = new List<(EntityUid Entity, EntityCoordinates Coordinates)>();
        var stations = _gameTicker.GetSpawnableStations();

        while (query.MoveNext(out var vendingUid, out _, out var xform))
        {
            var ownerStation = _station.GetOwningStation(vendingUid);

            if (ownerStation == null
                || ownerStation != stations[0])
                continue;

            // Make sure that we aren't running this on something that is already a mimic
            if (HasComp<CombatModeComponent>(vendingUid))
                continue;

            spawns.Add((vendingUid, xform.Coordinates));
        }

        if (spawns == null)
            //WTF: THE STATION DOESN'T EXIST! WE MUST BE IN A TEST! QUICK, PUT A MIMIC AT 0,0!!!
            Spawn(component.Proto, new EntityCoordinates(uid, new Vector2(0, 0)));
        else
        {
            // This is intentionally not clamped. If a server host wants to replace every vending machine in the entire station with a mimic, who am I to stop them?
            var k = MathF.MaxMagnitude(component.NumberToReplace, 1);
            while (k > 0 && spawns != null && spawns.Count > 0)
            {
                if (k > 1)
                {
                    var spawnLocation = _random.PickAndTake(spawns);
                    BuildAMimicWorkshop(spawnLocation.Entity, component);
                }
                else
                {
                    BuildAMimicWorkshop(spawns[0].Entity, component);
                }

                if (k == MathF.MaxMagnitude(component.NumberToReplace, 1)
                    && component.DoAnnouncement)
                    _chat.DispatchStationAnnouncement(stations[0], Loc.GetString("station-event-rampant-intelligence-announcement"), playDefaultSound: true,
                        colorOverride: Color.Red, sender: "Central Command");

                k--;
            }
        }
    }

    /// It's like Build a Bear, but MURDER
    public void BuildAMimicWorkshop(EntityUid uid, MobReplacementRuleComponent component)
    {
        var metaData = MetaData(uid);
        var vendorPrototype = metaData.EntityPrototype;
        var mimicProto = _prototype.Index(component.Proto);

        var vendorComponents = vendorPrototype?.Components.Keys
            .Where(n => n != "Transform" && n != "MetaData")
            .Select(name => (name, _componentFactory.GetRegistration(name).Type))
            .ToList() ?? new List<(string name, Type type)>();

        var mimicComponents = mimicProto?.Components.Keys
            .Where(n => n != "Transform" && n != "MetaData")
            .Select(name => (name, _componentFactory.GetRegistration(name).Type))
            .ToList() ?? new List<(string name, Type type)>();

        foreach (var name in mimicComponents.Except(vendorComponents))
        {
            var newComponent = _componentFactory.GetComponent(name.name);
            EntityManager.AddComponent(uid, newComponent);
        }

        var xform = Transform(uid);
        if (xform.Anchored)
            _transform.Unanchor(uid, xform);

        SetupMimicNPC(uid, component);

        if (TryComp<AdvertiseComponent>(uid, out var vendor)
            && component.VendorModify)
            SetupMimicVendor(uid, component, vendor);
    }

    /// This handles getting the entity ready to be a hostile NPC
    private void SetupMimicNPC(EntityUid uid, MobReplacementRuleComponent component)
    {
        _physics.SetBodyType(uid, BodyType.KinematicController);
        _npcFaction.AddFaction(uid, "SimpleHostile");

        var melee = EnsureComp<MeleeWeaponComponent>(uid);
        melee.Angle = 0;
        DamageSpecifier dspec = new()
        {
            DamageDict = new()
            {
                { "Blunt", component.MimicMeleeDamage }
            }
        };
        melee.Damage = dspec;

        var movementSpeed = EnsureComp<MovementSpeedModifierComponent>(uid);
        (movementSpeed.BaseSprintSpeed, movementSpeed.BaseWalkSpeed) = (component.MimicMoveSpeed, component.MimicMoveSpeed);

        var htn = EnsureComp<HTNComponent>(uid);
        htn.RootTask = new HTNCompoundTask() { Task = component.MimicAIType };
        htn.Blackboard.SetValue(NPCBlackboard.NavSmash, component.MimicSmashGlass);
        _npc.WakeNPC(uid, htn);
    }

    /// Handling specific interactions with vending machines
    private void SetupMimicVendor(EntityUid uid, MobReplacementRuleComponent mimicComponent, AdvertiseComponent vendorComponent)
    {
        vendorComponent.MinimumWait = 5;
        vendorComponent.MaximumWait = 15;
        _advertise.SayAdvertisement(uid, vendorComponent);

        if (TryComp<ApcPowerReceiverComponent>(uid, out var aPC))
            aPC.NeedsPower = false;
    }
}
