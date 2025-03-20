using Content.Shared.Access;
using Content.Shared.TurretController;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client.TurretController;

public sealed class TurretControllerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private TurretControllerWindow? _window;

    public TurretControllerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        if (UiKey is not DeployableTurretControllerUiKey)
        {
            Close();
            return;
        }

        _window = this.CreateWindow<TurretControllerWindow>();
        _window.SetOwner(Owner);
        _window.OpenCentered();

        _window.OnAccessLevelsChangedEvent += OnAccessLevelChanged;
        _window.OnArmamentSettingChangedEvent += OnArmamentSettingChanged;
    }

    /// <summary>
    /// Update state in this context is for when users open the controller it will populate with the last interfacestate
    /// </summary>
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null)
            return;

        if (state is not DeployableTurretControllerBoundInterfaceState { } cast)
            return;

        _window.UpdateState(cast);
    }

    /// <summary>
    /// ReceiveMessage is for live turret data to be updated continuously to the client UI.
    /// </summary>
    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        base.ReceiveMessage(message);

        if (_window == null)
            return;

        if (message is not DeployableTurretControllerBoundInterfaceMessage { } cast)
            return;

        // Update the turret states
        _window.UpdateMessage(cast);
    }

    /// <summary>
    /// When changed access level in UI send message to shared.
    /// </summary>
    private void OnAccessLevelChanged(HashSet<ProtoId<AccessLevelPrototype>> accessLevels, bool enabled)
    {
        SendPredictedMessage(new DeployableTurretExemptAccessLevelChangedMessage(accessLevels, enabled));
    }

    /// <summary>
    /// When changed armamentsettings in UI send message to shared.
    /// </summary>
    private void OnArmamentSettingChanged(int setting)
    {
        SendPredictedMessage(new DeployableTurretArmamentSettingChangedMessage(setting));
    }
}
