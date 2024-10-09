using Content.Shared.Targeting;
using Content.Shared.Targeting.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Targeting;
public sealed class TargetingSystem : SharedTargetingSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public event Action<TargetingComponent>? TargetingStartup;
    public event Action? TargetingShutdown;
    public event Action<TargetingComponent>? PartStatusStartup;
    public event Action<TargetingComponent>? PartStatusUpdate;
    public event Action? PartStatusShutdown;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TargetingComponent, LocalPlayerAttachedEvent>(HandlePlayerAttached);
        SubscribeLocalEvent<TargetingComponent, LocalPlayerDetachedEvent>(HandlePlayerDetached);
        SubscribeLocalEvent<TargetingComponent, ComponentStartup>(OnTargetingStartup);
        SubscribeLocalEvent<TargetingComponent, ComponentShutdown>(OnTargetingShutdown);
        SubscribeNetworkEvent<TargetIntegrityChangeEvent>(OnTargetIntegrityChange);
    }

    private void HandlePlayerAttached(EntityUid uid, TargetingComponent component, LocalPlayerAttachedEvent args)
    {
        TargetingStartup?.Invoke(component);
        PartStatusStartup?.Invoke(component);
    }

    private void HandlePlayerDetached(EntityUid uid, TargetingComponent component, LocalPlayerDetachedEvent args)
    {
        TargetingShutdown?.Invoke();
        PartStatusShutdown?.Invoke();
    }

    private void OnTargetingStartup(EntityUid uid, TargetingComponent component, ComponentStartup args)
    {
        if (_playerManager.LocalEntity == uid)
        {
            TargetingStartup?.Invoke(component);
            PartStatusStartup?.Invoke(component);
        }
    }

    private void OnTargetingShutdown(EntityUid uid, TargetingComponent component, ComponentShutdown args)
    {
        if (_playerManager.LocalEntity == uid)
        {
            TargetingShutdown?.Invoke();
            PartStatusShutdown?.Invoke();
        }
    }

    private void OnTargetIntegrityChange(TargetIntegrityChangeEvent args)
    {
        if (!TryGetEntity(args.Uid, out var uid)
        || !_playerManager.LocalEntity.Equals(uid)
        || !TryComp(uid, out TargetingComponent? component)
        || !args.RefreshUi)
            return;

        PartStatusUpdate?.Invoke(component);
    }
}