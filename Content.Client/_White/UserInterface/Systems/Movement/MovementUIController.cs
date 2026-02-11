using Content.Client._White.UserInterface.Systems.Movement.Widgets;
using Content.Client.Gameplay;
using Content.Client.Physics.Controllers;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client._White.UserInterface.Systems.Movement;

public sealed class MovementUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<MoverController>
{
    private InputMoverComponent? _inputMoverComponent;

    private MovementGui? MovementGui => UIManager.GetActiveUIWidgetOrNull<MovementGui>();

    public void OnSystemLoaded(MoverController system)
    {
        system.LocalPlayerInputMoverUpdated += OnInputMoverUpdated;
        system.LocalPlayerInputMoverAdded += OnInputMoverAdded;
        system.LocalPlayerInputMoverRemoved += OnInputMoverRemoved;
    }

    public void OnSystemUnloaded(MoverController system)
    {
        system.LocalPlayerInputMoverUpdated -= OnInputMoverUpdated;
        system.LocalPlayerInputMoverAdded -= OnInputMoverAdded;
        system.LocalPlayerInputMoverRemoved -= OnInputMoverRemoved;
    }

    public void OnStateEntered(GameplayState state)
    {
        if (MovementGui != null)
            MovementGui.Visible = _inputMoverComponent is not null;
    }

    private void OnInputMoverUpdated(bool sprinting)
    {
        MovementGui?.OnInputMoverUpdated(sprinting);
    }

    private void OnInputMoverAdded(InputMoverComponent component)
    {
        if (MovementGui != null)
            MovementGui.Visible = true;

        _inputMoverComponent = component;
        OnInputMoverUpdated(component.Sprinting);
    }

    private void OnInputMoverRemoved()
    {
        if (MovementGui != null)
            MovementGui.Visible = false;

        _inputMoverComponent = null;
    }

    public void ToggleInputMover() => EntityManager.RaisePredictiveEvent(new ToggleInputMoverRequestEvent());
}
