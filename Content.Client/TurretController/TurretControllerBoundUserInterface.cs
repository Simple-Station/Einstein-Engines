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

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null)
            return;

        if (state is not DeployableTurretControllerBoundInterfaceState { } cast)
            return;

        _window.UpdateState(cast);
    }

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

    private void OnAccessLevelChanged(HashSet<ProtoId<AccessLevelPrototype>> accessLevels, bool enabled)
    {
        SendPredictedMessage(new DeployableTurretExemptAccessLevelChangedMessage(accessLevels, enabled));
    }

    private void OnArmamentSettingChanged(int setting)
    {
        SendPredictedMessage(new DeployableTurretArmamentSettingChangedMessage(setting));
    }
}
