using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Alert;
using JetBrains.Annotations;

namespace Content.Server.Alert.Click;

/// <summary>
/// Attempts to toggle the internals for a particular entity
/// </summary>
[UsedImplicitly]
[DataDefinition]
public sealed partial class ToggleWalking : IAlertClick
{
    public void AlertClicked(EntityUid player)
    {
        var movementSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SharedMoverController>();
        var entManager = IoCManager.Resolve<IEntityManager>();
        entManager.TryGetComponent<InputMoverComponent>(player, out var mover);
        movementSystem.ToggleWalking(player);
    }
}
