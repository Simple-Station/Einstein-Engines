﻿using Content.Server.Atmos.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Managers;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Mind;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Temperature.Components;
using Content.Shared.Atmos;
using Content.Shared._Goobstation.Blob;
using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.Inventory;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Physics;
using Content.Shared.Tag;
using Content.Shared.Zombies;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Blob.Systems;

public sealed class ZombieBlobSystem : SharedZombieBlobSystem
{
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IChatManager _chatMan = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    private const int ClimbingCollisionGroup = (int) CollisionGroup.BlobImpassable;

    private readonly GasMixture _normalAtmos;
    public ZombieBlobSystem()
    {
        _normalAtmos = new GasMixture(Atmospherics.CellVolume)
        {
            Temperature = Atmospherics.T20C
        };
        _normalAtmos.AdjustMoles(Gas.Oxygen, Atmospherics.OxygenMolesStandard);
        _normalAtmos.AdjustMoles(Gas.Nitrogen, Atmospherics.NitrogenMolesStandard);
        _normalAtmos.MarkImmutable();
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ZombieBlobComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<ZombieBlobComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ZombieBlobComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ZombieBlobComponent, InhaleLocationEvent>(OnInhale);
        SubscribeLocalEvent<ZombieBlobComponent, ExhaleLocationEvent>(OnExhale);

    }

    private void OnInhale(Entity<ZombieBlobComponent> ent, ref InhaleLocationEvent args) => args.Gas = _normalAtmos;
    private void OnExhale(Entity<ZombieBlobComponent> ent, ref ExhaleLocationEvent args) => args.Gas = GasMixture.SpaceGas;

    /// <summary>
    /// Replaces the current fixtures with non-climbing collidable versions so that climb end can be detected
    /// </summary>
    /// <returns>Returns whether adding the new fixtures was successful</returns>
    private void ReplaceFixtures(EntityUid uid, ZombieBlobComponent climbingComp, FixturesComponent fixturesComp)
    {
        foreach (var (name, fixture) in fixturesComp.Fixtures)
        {
            if (climbingComp.DisabledFixtureMasks.ContainsKey(name)
                || fixture.Hard == false
                || (fixture.CollisionMask & ClimbingCollisionGroup) == 0)
                continue;

            climbingComp.DisabledFixtureMasks.Add(name, fixture.CollisionMask & ClimbingCollisionGroup);
            _physics.SetCollisionMask(uid, name, fixture, fixture.CollisionMask & ~ClimbingCollisionGroup, fixturesComp);
        }
    }

    private void OnStartup(EntityUid uid, ZombieBlobComponent component, ComponentStartup args)
    {
        _ui.CloseUis(uid);
        _inventory.TryUnequip(uid, "underpants", true, true);
        _inventory.TryUnequip(uid, "neck", true, true);
        _inventory.TryUnequip(uid, "mask", true, true);
        _inventory.TryUnequip(uid, "eyes", true, true);
        _inventory.TryUnequip(uid, "ears", true, true);

        EnsureComp<BlobMobComponent>(uid);
        EnsureComp<BlobSpeakComponent>(uid);

        var oldFactions = new List<string>();
        var factionComp = EnsureComp<NpcFactionMemberComponent>(uid);
        foreach (var factionId in new List<ProtoId<NpcFactionPrototype>>(factionComp.Factions))
        {
            oldFactions.Add(factionId);
            _faction.RemoveFaction(uid, factionId);
        }
        _faction.AddFaction(uid, "Blob");
        component.OldFactions = oldFactions;

        _tagSystem.AddTag(uid, "BlobMob");

        EnsureComp<PressureImmunityComponent>(uid);

        if (TryComp<TemperatureComponent>(uid, out var temperatureComponent))
        {
            component.OldColdDamageThreshold = temperatureComponent.ColdDamageThreshold;
            temperatureComponent.ColdDamageThreshold = 0;
        }

        if (TryComp<FixturesComponent>(uid, out var fixturesComp))
        {
            ReplaceFixtures(uid, component, fixturesComp);
        }

        var mindComp = EnsureComp<MindContainerComponent>(uid);
        if (mindComp.Mind != null)
        {
            /*
            if (!_roleSystem.MindHasRole<BlobRoleComponent>(mindComp.Mind.Value))
            {
                _roleSystem.MindAddRole(mindComp.Mind.Value, new BlobRoleComponent
                {
                    PrototypeId = "Blob"
                });
            }*/

            if (_mind.TryGetSession(mindComp.Mind, out var session))
            {
                _chatMan.DispatchServerMessage(session, Loc.GetString("blob-zombie-greeting"));
                _audio.PlayGlobal(component.GreetSoundNotification, session);
            }
        }
        else
        {
            var htn = EnsureComp<HTNComponent>(uid);
            htn.RootTask = new HTNCompoundTask() {Task = "SimpleHostileCompound"};
            htn.Blackboard.SetValue(NPCBlackboard.Owner, uid);
            htn.Blackboard.SetValue(NPCBlackboard.NavBlob, true);

            if (!HasComp<ActorComponent>(component.BlobPodUid))
            {
                _npc.WakeNPC(uid, htn);
            }
        }

        var ev = new EntityZombifiedEvent(uid);
        RaiseLocalEvent(uid, ref ev, true);
    }

    private void OnShutdown(EntityUid uid, ZombieBlobComponent component, ComponentShutdown args)
    {
        if (TerminatingOrDeleted(uid))
            return;

        _ui.CloseUis(uid);
        RemComp<BlobSpeakComponent>(uid);
        RemComp<BlobMobComponent>(uid);
        RemComp<HTNComponent>(uid);
        RemComp<PressureImmunityComponent>(uid);

        if (TryComp<TemperatureComponent>(uid, out var temperatureComponent) && component.OldColdDamageThreshold != null)
        {
            temperatureComponent.ColdDamageThreshold = component.OldColdDamageThreshold.Value;
        }

        _tagSystem.RemoveTag(uid, "BlobMob");

        /*
        var mindComp = EnsureComp<MindContainerComponent>(uid);
        if (mindComp.Mind != null)
        {
            _roleSystem.MindTryRemoveRole<BlobRoleComponent>(mindComp.Mind.Value);
        }
        */

        _trigger.Trigger(component.BlobPodUid);
        QueueDel(component.BlobPodUid);

        EnsureComp<NpcFactionMemberComponent>(uid);
        foreach (var factionId in component.OldFactions)
        {
            _faction.AddFaction(uid, factionId);
        }
        _faction.RemoveFaction(uid, "Blob");

        if (TryComp<FixturesComponent>(uid, out var fixtures))
        {
            foreach (var (name, fixtureMask) in component.DisabledFixtureMasks)
            {
                if (!fixtures.Fixtures.TryGetValue(name, out var fixture))
                {
                    continue;
                }

                _physics.SetCollisionMask(uid, name, fixture, fixture.CollisionMask | fixtureMask, fixtures);
            }
            component.DisabledFixtureMasks.Clear();
        }
    }

    private void OnMobStateChanged(EntityUid uid, ZombieBlobComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
        {
            RemComp<ZombieBlobComponent>(uid);
        }
    }
}
