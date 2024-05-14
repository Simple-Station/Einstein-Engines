using System.Threading;

using Content.Server.Polymorph.Components;

namespace Content.Server.Polymorph.Systems;

/// <summary>
/// This handles polymorphing entity after time
/// </summary>
public sealed class TimedPolymorphSystem : EntitySystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TimedPolymorphComponent, ComponentInit>(OnPolymorphInit);
        SubscribeLocalEvent<TimedPolymorphComponent, ComponentShutdown>(OnTimedPolymorphShutdown);
    }

    private void OnPolymorphInit(EntityUid uid, TimedPolymorphComponent component, ComponentInit args)
    {
        component.TokenSource?.Cancel();
        component.TokenSource = new CancellationTokenSource();
        uid.SpawnRepeatingTimer(TimeSpan.FromSeconds(component.PolymorphTime), () => OnTimerFired(uid, component), component.TokenSource.Token);
    }

    private void OnTimerFired(EntityUid uid, TimedPolymorphComponent component)
    {
        if (component.Enabled)
            _polymorph.PolymorphEntity(uid, component.PolymorphPrototype);
    }

    private void OnTimedPolymorphShutdown(EntityUid uid, TimedPolymorphComponent component, ComponentShutdown args)
    {
        component.TokenSource?.Cancel();
    }
}
