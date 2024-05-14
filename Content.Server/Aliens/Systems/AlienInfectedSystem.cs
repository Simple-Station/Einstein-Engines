using System.Linq;
using System.Threading;
using Content.Server.Aliens.Components;
using Content.Server.Body.Systems;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Mind;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Ghost.Roles;
using Content.Shared.Gibbing.Components;
using Content.Shared.Gibbing.Events;
using Content.Shared.Gibbing.Systems;
using Content.Shared.Mobs;
using Content.Shared.Random;
using FastAccessors;
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
    [Dependency] private readonly GhostRoleSystem _ghostRole = default!;
    [Dependency] private readonly MindSystem _mind = default!;
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

            if (infected.GrowthStage == 5)
            {
                Spawn(infected.Prototype, Transform(uid).Coordinates);
                _body.GibBody(uid, true);
            }

            if (_random.Prob(infected.GrowProb))
            {
                infected.GrowthStage++;
            }
            infected.NextGrowRoll = _timing.CurTime + TimeSpan.FromSeconds(infected.GrowTime);
        }
    }
}
