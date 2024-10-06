using Content.Shared.Targeting;
using Content.Shared.Targeting.Events;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Targeting;
public sealed class TargetingSystem : SharedTargetingSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    public event Action<TargetingComponent>? TargetingStartup;
    public event Action? TargetingShutdown;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TargetingComponent, LocalPlayerAttachedEvent>(HandlePlayerAttached);
        SubscribeLocalEvent<TargetingComponent, LocalPlayerDetachedEvent>(HandlePlayerDetached);
        SubscribeLocalEvent<TargetingComponent, ComponentStartup>(OnTargetingStartup);
        SubscribeLocalEvent<TargetingComponent, ComponentShutdown>(OnTargetingShutdown);
    }

    private void HandlePlayerAttached(EntityUid uid, TargetingComponent component, LocalPlayerAttachedEvent args)
    {
        TargetingStartup?.Invoke(component);
    }

    private void HandlePlayerDetached(EntityUid uid, TargetingComponent component, LocalPlayerDetachedEvent args)
    {
        TargetingShutdown?.Invoke();
    }

    private void OnTargetingStartup(EntityUid uid, TargetingComponent component, ComponentStartup args)
    {
        if (_playerManager.LocalEntity == uid)
            TargetingStartup?.Invoke(component);
    }

    private void OnTargetingShutdown(EntityUid uid, TargetingComponent component, ComponentShutdown args)
    {
        if (_playerManager.LocalEntity == uid)
            TargetingShutdown?.Invoke();
    }
}