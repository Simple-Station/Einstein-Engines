using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.Access;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Systems;
using Content.Shared.TurretController;
using Content.Shared.Turrets;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server.TurretController;

public sealed partial class DeployableTurretControllerSystem : SharedDeployableTurretControllerSystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetwork = default!;

    public const string CmdSetArmamemtState = "set_armament_state";
    public const string CmdSetAccessExemptions = "set_access_exemption";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeployableTurretControllerComponent, BoundUIOpenedEvent>(OnBUIOpened);
        SubscribeLocalEvent<DeployableTurretControllerComponent, DeviceListUpdateEvent>(OnDeviceListUpdate);
        SubscribeLocalEvent<DeployableTurretControllerComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
    }

    private void OnBUIOpened(Entity<DeployableTurretControllerComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUIState(ent);
    }

    /// <summary>
    /// Each time you add devices to the controller it will update all turrets.
    /// </summary>
    private void OnDeviceListUpdate(Entity<DeployableTurretControllerComponent> ent, ref DeviceListUpdateEvent args)
    {
        if (!TryComp<DeviceNetworkComponent>(ent, out var deviceNetwork))
            return;

        // List of turrets
        var turretsToAdd = args.Devices;

        // Refresh turrets
        ent.Comp.LinkedTurrets.Clear();

        // Request data from newly added devices
        var payload = new NetworkPayload
        {
            [DeviceNetworkConstants.Command] = DeviceNetworkConstants.CmdUpdatedState,
        };

        foreach (var turretUid in turretsToAdd)
        {
            if (!HasComp<DeployableTurretComponent>(turretUid))
                continue;

            if (!TryComp<DeviceNetworkComponent>(turretUid, out var turretDeviceNetwork))
                continue;

            _deviceNetwork.QueuePacket(ent, turretDeviceNetwork.Address, payload, device: deviceNetwork);
        }
    }

    /// <summary>
    /// When recieving the packet back from the turret it will update the current updatedstates.
    /// </summary>
    private void OnPacketReceived(Entity<DeployableTurretControllerComponent> ent, ref DeviceNetworkPacketEvent args)
    {
        if (!args.Data.TryGetValue(DeviceNetworkConstants.Command, out string? command))
            return;

        // If an update is received from a conencted turrt, update the UI
        if (command == DeviceNetworkConstants.CmdUpdatedState &&
            args.Data.TryGetValue(command, out DeployableTurretState updatedState))
        {
            ent.Comp.LinkedTurrets[args.SenderAddress] = updatedState;
            UpdateUIState(ent);
        }
    }

    /// <summary>
    /// When you change the armament setting send a packet to the turrets to update them.
    /// </summary>
    protected override void ChangeArmamentSetting(Entity<DeployableTurretControllerComponent> ent, int armamentState, EntityUid? user = null)
    {
        base.ChangeArmamentSetting(ent, armamentState, user);

        if (!TryComp<DeviceNetworkComponent>(ent, out var device))
            return;

        // Update linked turrets' armament statuses
        var payload = new NetworkPayload
        {
            [DeviceNetworkConstants.Command] = CmdSetArmamemtState,
            [CmdSetArmamemtState] = armamentState,
        };

        _deviceNetwork.QueuePacket(ent, null, payload, device: device);
    }

    /// <summary>
    /// When changing the access levels send another packet to the turrets.
    /// </summary>
    protected override void ChangeExemptAccessLevels
        (Entity<DeployableTurretControllerComponent> ent, HashSet<ProtoId<AccessLevelPrototype>> exemptions, bool enabled, EntityUid? user = null)
    {
        base.ChangeExemptAccessLevels(ent, exemptions, enabled, user);

        if (!TryComp<DeviceNetworkComponent>(ent, out var device) ||
            !TryComp<TurretTargetSettingsComponent>(ent, out var turretTargetingSettings))
            return;

        // Update linked turrets' target selection exemptions
        var payload = new NetworkPayload
        {
            [DeviceNetworkConstants.Command] = CmdSetAccessExemptions,
            [CmdSetAccessExemptions] = turretTargetingSettings.ExemptAccessLevels,
        };

        _deviceNetwork.QueuePacket(ent, null, payload, device: device);
    }

    /// <summary>
    /// Updates the UI state for the user that currently has it open.
    /// </summary>
    private void UpdateUIState(Entity<DeployableTurretControllerComponent> ent)
    {
        var turretStates = new List<(string, string)>();

        foreach (var (address, turret) in ent.Comp.LinkedTurrets)
            turretStates.Add((address, GetTurretStateDescription(turret)));

        // Live turret data.
        var message = new DeployableTurretControllerBoundInterfaceMessage(turretStates);
        _userInterfaceSystem.ServerSendUiMessage(ent.Owner, DeployableTurretControllerUiKey.Key, message);

        // For when it changes states or opens the UI.
        var state = new DeployableTurretControllerBoundInterfaceState(turretStates);
        _userInterfaceSystem.SetUiState(ent.Owner, DeployableTurretControllerUiKey.Key, state);
    }

    private string GetTurretStateDescription(DeployableTurretState state)
    {
        switch (state)
        {
            case DeployableTurretState.Disabled:
                return "turret-controls-window-turret-disabled";
            case DeployableTurretState.Firing:
                return "turret-controls-window-turret-firing";
            case DeployableTurretState.Deploying:
                return "turret-controls-window-turret-deploying";
            case DeployableTurretState.Deployed:
                return "turret-controls-window-turret-deployed";
            case DeployableTurretState.Retracting:
                return "turret-controls-window-turret-retracting";
            case DeployableTurretState.Retracted:
                return "turret-controls-window-turret-retracted";
        }

        return "turret-controls-window-turret-error";
    }
}
