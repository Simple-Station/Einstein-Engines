using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.Targeting.Widgets;
using Content.Shared.Targeting;
using Content.Client.Targeting;
using Content.Shared.Targeting.Events;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.Player;

namespace Content.Client.UserInterface.Systems.Targeting;

public sealed class PartStatusUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<PartStatusSystem>
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IEntityNetworkManager _net = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private TargetingComponent? _targetingComponent;
    private PartStatusControl? PartStatusControl => UIManager.GetActiveUIWidgetOrNull<PartStatusControl>();

    public void OnSystemLoaded(PartStatusSystem system)
    {
        system.PartStatusStartup += AddPartStatusControl;
        system.PartStatusShutdown += RemovePartStatusControl;
    }

    public void OnSystemUnloaded(PartStatusSystem system)
    {
        system.PartStatusStartup -= AddPartStatusControl;
        system.PartStatusShutdown -= RemovePartStatusControl;
    }

    public void OnStateEntered(GameplayState state)
    {
        if (TargetingControl != null)
        {
            TargetingControl.SetVisible(_targetingComponent != null);

            if (_targetingComponent != null)
                TargetingControl.SetColors(_targetingComponent.Target);
        }
    }

    public void AddTargetingControl(TargetingComponent component)
    {
        _targetingComponent = component;

        if (TargetingControl != null)
        {
            TargetingControl.SetVisible(_targetingComponent != null);

            if (_targetingComponent != null)
                TargetingControl.SetColors(_targetingComponent.Target);
        }

    }

    public void RemoveTargetingControl()
    {
        if (TargetingControl != null)
            TargetingControl.SetVisible(false);

        _targetingComponent = null;
    }

    public void CycleTarget(TargetBodyPart bodyPart, TargetingControl control)
    {
        if (_playerManager.LocalEntity is not { } user
        || _entManager.GetComponent<TargetingComponent>(user) is not { } targetingComponent)
            return;

        var player = _entManager.GetNetEntity(user);
        if (bodyPart != targetingComponent.Target)
        {
            var msg = new TargetChangeEvent(player, bodyPart);
            _net.SendSystemNetworkMessage(msg);
            control.SetColors(bodyPart);
        }
    }


}