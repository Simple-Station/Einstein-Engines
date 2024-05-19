using System.Linq;
using System.Threading;
using Content.Server.Aliens.Components;
using Content.Server.Body.Systems;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Mind;
using Content.Shared.Aliens.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Ghost.Roles;
using Content.Shared.Gibbing.Components;
using Content.Shared.Gibbing.Events;
using Content.Shared.Gibbing.Systems;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Random;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using FastAccessors;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using AlienInfectedComponent = Content.Shared.Aliens.Components.AlienInfectedComponent;

namespace Content.Server.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class AlienInfectedSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AlienInfectedComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<AlienInfectedComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentInit(EntityUid uid, AlienInfectedComponent component, ComponentInit args)
    {
        // var torsoPart = Comp<BodyComponent>(uid).RootContainer.ContainedEntities[0];
        // _body.TryCreateOrganSlot(torsoPart, "alienLarvaOrgan", out _);
        // _body.InsertOrgan(torsoPart, Spawn(component.OrganProtoId, Transform(uid).Coordinates), "alienLarvaOrgan");
        component.NextGrowRoll = _timing.CurTime + TimeSpan.FromSeconds(component.GrowTime);
        component.Stomach = _container.EnsureContainer<Container>(uid, "stomach");
    }

    private void OnComponentShutdown(EntityUid uid, AlienInfectedComponent component, ComponentShutdown args)
    {

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AlienInfectedComponent>();
        while (query.MoveNext(out var uid, out var infected))
        {
            if (_timing.CurTime < infected.NextGrowRoll)
                continue;

            if (HasComp<InsideAlienLarvaComponent>(infected.SpawnedLarva) &&
                Comp<InsideAlienLarvaComponent>(infected.SpawnedLarva).IsGrown)
            {
                _container.EmptyContainer(infected.Stomach);
                _body.GibBody(uid, true, CompOrNull<BodyComponent>(uid), true, null, 5f);
            }

            if (infected.GrowthStage == 5)
            {
                var larva = Spawn(infected.Prototype, Transform(uid).Coordinates);
                _container.Insert(larva, infected.Stomach);
                infected.SpawnedLarva = larva;
                infected.GrowthStage++;
            }

            if (_random.Prob(infected.GrowProb))
            {
                infected.GrowthStage++;
            }
            infected.NextGrowRoll = _timing.CurTime + TimeSpan.FromSeconds(infected.GrowTime);
        }
    }
}
