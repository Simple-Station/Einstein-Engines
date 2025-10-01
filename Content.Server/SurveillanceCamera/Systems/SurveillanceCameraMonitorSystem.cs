using System.Linq;
using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Power.Components;
using Content.Shared._Goobstation.SurveillanceCamera;
using Content.Shared.DeviceNetwork;
using Content.Shared.Power;
using Content.Shared.UserInterface;
using Content.Shared.SurveillanceCamera;
using Content.Shared.UserInterface; // Goobstation
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Map; // Goobstation
using Robust.Shared.Player; // Goobstation

namespace Content.Server.SurveillanceCamera;

public sealed class SurveillanceCameraMonitorSystem : EntitySystem
{
    [Dependency] private readonly SurveillanceCameraSystem _surveillanceCameras = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SurveillanceCameraMonitorComponent, SurveillanceCameraDeactivateEvent>(OnSurveillanceCameraDeactivate);
        SubscribeLocalEvent<SurveillanceCameraMonitorComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<SurveillanceCameraMonitorComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
        // SubscribeLocalEvent<SurveillanceCameraMonitorComponent, ComponentStartup>(OnComponentStartup); // Goobstation remove
        SubscribeLocalEvent<SurveillanceCameraMonitorComponent, AfterActivatableUIOpenEvent>(OnToggleInterface);
        Subs.BuiEvents<SurveillanceCameraMonitorComponent>(SurveillanceCameraMonitorUiKey.Key, subs =>
        {
            subs.Event<SurveillanceCameraRefreshCamerasMessage>(OnRefreshCamerasMessage);
            subs.Event<SurveillanceCameraRefreshSubnetsMessage>(OnRefreshSubnetsMessage);
            subs.Event<SurveillanceCameraDisconnectMessage>(OnDisconnectMessage);
            subs.Event<SurveillanceCameraMonitorSwitchMessage>(OnSwitchMessage);
            subs.Event<BoundUIClosedEvent>(OnBoundUiClose);
        });
    }

    private const float _maxHeartbeatTime = 300f;
    private const float _heartbeatDelay = 30f;

    public override void Update(float frameTime)
    {
        var query =
            EntityQueryEnumerator<ActiveSurveillanceCameraMonitorComponent, SurveillanceCameraMonitorComponent>();
        while (query.MoveNext(out var uid, out _, out var monitor))
        {
            /*if (Paused(uid))
            {
                continue;
            } // Goobstation remove */

            monitor.LastHeartbeatSent += frameTime;
            SendHeartbeat(uid, monitor);
            monitor.LastHeartbeat += frameTime;

            if (monitor.LastHeartbeat > _maxHeartbeatTime)
            {
                DisconnectCamera(uid, true, monitor);
                RemComp<ActiveSurveillanceCameraMonitorComponent>(uid); // Goobstation
            }
        }

        // Goobstation start
        var queryTwo =
            EntityQueryEnumerator<ReconnectingSurveillanceCameraMonitorComponent, SurveillanceCameraMonitorComponent>();
        while (queryTwo.MoveNext(out var uid, out var reconnectingComponent, out var monitor))
        {
            if (reconnectingComponent.TicksDelay-- == 0)
            {
                ReconnectToSubnets(uid, monitor);
                RemComp<ReconnectingSurveillanceCameraMonitorComponent>(uid);
            }
        }
        // Goobstation end
    }


    /// ROUTING:
    ///
    /// Monitor freq: General frequency for cameras, routers, and monitors to speak on.
    ///
    /// Subnet freqs: Frequency for each specific subnet. Routers ping cameras here,
    ///               cameras ping back on monitor frequency. When a monitor
    ///               selects a subnet, it saves that subnet's frequency
    ///               so it can connect to the camera. All outbound cameras
    ///               always speak on the monitor frequency and will not
    ///               do broadcast pings - whatever talks to it, talks to it.
    ///
    /// How a camera is discovered:
    ///
    /// Subnet ping:
    /// Surveillance camera monitor - [ monitor freq ] -> Router
    /// Router -> camera discovery
    /// Router - [ subnet freq ] -> Camera
    /// Camera -> router ping
    /// Camera - [ monitor freq ] -> Router
    /// Router -> monitor data forward
    /// Router - [ monitor freq ] -> Monitor

    #region Event Handling
    /*private void OnComponentStartup(EntityUid uid, SurveillanceCameraMonitorComponent component, ComponentStartup args)
    {
        RefreshSubnets(uid, component);
    } Goobstation remove */

    private void OnPacketReceived(EntityUid uid, SurveillanceCameraMonitorComponent component,
        DeviceNetworkPacketEvent args)
    {
        if (string.IsNullOrEmpty(args.SenderAddress))
        {
            return;
        }

        if (args.Data.TryGetValue(DeviceNetworkConstants.Command, out string? command))
        {
            switch (command)
            {
                case SurveillanceCameraSystem.CameraConnectMessage:
                    if (component.NextCameraAddress == args.SenderAddress)
                    {
                        component.ActiveCameraAddress = args.SenderAddress;
                        TrySwitchCameraByUid(uid, args.Sender, component);
                    }

                    component.NextCameraAddress = null;
                    break;
                case SurveillanceCameraSystem.CameraHeartbeatMessage:
                    if (args.SenderAddress == component.ActiveCameraAddress)
                    {
                        component.LastHeartbeat = 0;
                    }

                    break;
                case SurveillanceCameraSystem.CameraDataMessage:
                    if (!args.Data.TryGetValue(SurveillanceCameraSystem.CameraNameData, out string? name)
                        || !args.Data.TryGetValue(SurveillanceCameraSystem.CameraSubnetData, out string? subnetData)
                        || !args.Data.TryGetValue(SurveillanceCameraSystem.CameraAddressData, out string? address)
                        || !args.Data.TryGetValue(SurveillanceCameraSystem.CameraNetEntity, out (NetEntity, NetCoordinates)? netEntity)) // Goobstation
                    {
                        return;
                    }
                    if (!component.KnownCameras.ContainsKey(address))
                    {
                        component.KnownCameras.Add(address, netEntity.Value); // Goobstation
                    }

                    UpdateUserInterface(uid, component);
                    break;
                case SurveillanceCameraSystem.CameraSubnetData:
                    if (args.Data.TryGetValue(SurveillanceCameraSystem.CameraSubnetData, out string? subnet)
                        && !string.IsNullOrEmpty(subnet)
                        && !component.KnownSubnets.ContainsKey(subnet))
                    {
                        component.KnownSubnets.Add(subnet, args.SenderAddress);
                    }

                    UpdateUserInterface(uid, component);
                    break;
            }
        }
    }

    private void OnDisconnectMessage(EntityUid uid, SurveillanceCameraMonitorComponent component,
        SurveillanceCameraDisconnectMessage message)
    {
        DisconnectCamera(uid, true, component);
    }

    private void OnRefreshCamerasMessage(EntityUid uid, SurveillanceCameraMonitorComponent component,
        SurveillanceCameraRefreshCamerasMessage message)
    {
        component.KnownCameras.Clear();
        RequestKnownSubnetsInfo(uid, component); // Goobstation
    }

    private void OnRefreshSubnetsMessage(EntityUid uid, SurveillanceCameraMonitorComponent component,
        SurveillanceCameraRefreshSubnetsMessage message)
    {
        RefreshSubnets(uid, component);
    }

    private void OnSwitchMessage(EntityUid uid, SurveillanceCameraMonitorComponent component, SurveillanceCameraMonitorSwitchMessage message)
    {
        // there would be a null check here, but honestly
        // whichever one is the "latest" switch message gets to
        // do the switch
        TrySwitchCameraByAddress(uid, message.Address, component);
    }

    private void OnPowerChanged(EntityUid uid, SurveillanceCameraMonitorComponent component, ref PowerChangedEvent args)
    {
        if (!args.Powered)
        {
            RemoveActiveCamera(uid, component);
            component.NextCameraAddress = null;
            // Goobstation start

            foreach (var subnetwork in component.KnownSubnets.Values)
                DisconnectFromSubnet(uid, subnetwork);
            // Goobstation end
        }
    }

    private void OnToggleInterface(EntityUid uid, SurveillanceCameraMonitorComponent component,
        AfterActivatableUIOpenEvent args)
    {
        AfterOpenUserInterface(uid, args.User, component);
    }

    // This is to ensure that there's no delay in ensuring that a camera is deactivated.
    private void OnSurveillanceCameraDeactivate(EntityUid uid, SurveillanceCameraMonitorComponent monitor, SurveillanceCameraDeactivateEvent args)
    {
        DisconnectCamera(uid, false, monitor);
    }

    private void OnBoundUiClose(EntityUid uid, SurveillanceCameraMonitorComponent component, BoundUIClosedEvent args)
    {
        RemoveViewer(uid, args.Actor, component);
    }

    #endregion

    private void SendHeartbeat(EntityUid uid, SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor)
            || monitor.LastHeartbeatSent < _heartbeatDelay) // Goobstation
        {
            return;
        }

        // Goobstation start
        foreach (var subnetAddress in monitor.KnownSubnets.Values)
        {
            var payload = new NetworkPayload()
            {
                { DeviceNetworkConstants.Command, SurveillanceCameraSystem.CameraHeartbeatMessage },
                { SurveillanceCameraSystem.CameraAddressData, monitor.ActiveCameraAddress }
            };

            _deviceNetworkSystem.QueuePacket(uid, subnetAddress, payload);
        }
        // Goobstation end
    }

    private void DisconnectCamera(EntityUid uid, bool removeViewers, SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor))
        {
            return;
        }

        if (removeViewers)
        {
            RemoveActiveCamera(uid, monitor);
        }

        monitor.ActiveCamera = null;
        monitor.ActiveCameraAddress = string.Empty;
        RemComp<ActiveSurveillanceCameraMonitorComponent>(uid); // Goobstation
        UpdateUserInterface(uid, monitor);
    }

    private void RefreshSubnets(EntityUid uid, SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor)
            || HasComp<ReconnectingSurveillanceCameraMonitorComponent>(uid)) // Goobstation
        {
            return;
        }

        // Goobstation start
        foreach (var subnetAddress in monitor.KnownSubnets.Values)
        {
            var payload = new NetworkPayload()
            {
                {DeviceNetworkConstants.Command, SurveillanceCameraSystem.CameraSubnetDisconnectMessage},
            };
            _deviceNetworkSystem.QueuePacket(uid, subnetAddress, payload);
        }
        // Goobstation end

        monitor.KnownSubnets.Clear();
        PingCameraNetwork(uid, monitor);

        EnsureComp<ReconnectingSurveillanceCameraMonitorComponent>(uid); // Goobstation
    }

    // Goobstation start
    private void ReconnectToSubnets(EntityUid uid, SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor))
        {
            return;
        }
        foreach (var subnetAddress in monitor.KnownSubnets.Values)
        {
            var payload = new NetworkPayload()
            {
                {DeviceNetworkConstants.Command, SurveillanceCameraSystem.CameraSubnetConnectMessage},
            };
            _deviceNetworkSystem.QueuePacket(uid, subnetAddress, payload);
        }
    }
    // Goobstation end

    // Goobstation start
    private void PingCameraNetwork(EntityUid uid, SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor))
        {
            return;
        }

        var payload = new NetworkPayload()
        {
            { DeviceNetworkConstants.Command, SurveillanceCameraSystem.CameraPingMessage }
        };
        _deviceNetworkSystem.QueuePacket(uid, null, payload);
    }
    // Goobstation end

    // Goobstation start
    private void RequestKnownSubnetsInfo(EntityUid uid, SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor))
        {
            return;
        }

        foreach (var subnetAddress in monitor.KnownSubnets.Values)
        {
            var payload = new NetworkPayload()
            {
                {DeviceNetworkConstants.Command, SurveillanceCameraSystem.CameraPingSubnetMessage},
            };
            _deviceNetworkSystem.QueuePacket(uid, subnetAddress, payload);
        }
    }
    // Goobstation end

    private void DisconnectFromSubnet(EntityUid uid, string subnet, SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor)
            || string.IsNullOrEmpty(subnet)
            || !monitor.KnownSubnets.TryGetValue(subnet, out var address))
        {
            return;
        }

        var payload = new NetworkPayload()
        {
            {DeviceNetworkConstants.Command, SurveillanceCameraSystem.CameraSubnetDisconnectMessage},
        };
        _deviceNetworkSystem.QueuePacket(uid, address, payload);
    }

    // Adds a viewer to the camera and the monitor.
    private void AddViewer(EntityUid uid, EntityUid player, SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor))
        {
            return;
        }

        monitor.Viewers.Add(player);

        if (monitor.ActiveCamera != null)
        {
            _surveillanceCameras.AddActiveViewer(monitor.ActiveCamera.Value, player, uid);
        }

        UpdateUserInterface(uid, monitor, player);
    }

    // Removes a viewer from the camera and the monitor.
    private void RemoveViewer(EntityUid uid, EntityUid player, SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor))
        {
            return;
        }

        monitor.Viewers.Remove(player);

        if (monitor.ActiveCamera != null)
        {
            _surveillanceCameras.RemoveActiveViewer(monitor.ActiveCamera.Value, player);
        }
    }

    // Sets the camera. If the camera is not null, this will return.
    //
    // The camera should always attempt to switch over, rather than
    // directly setting it, so that the active viewer list and view
    // subscriptions can be updated.
    private void SetCamera(EntityUid uid, EntityUid camera, SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor)
            || monitor.ActiveCamera != null)
        {
            return;
        }

        _surveillanceCameras.AddActiveViewers(camera, monitor.Viewers, uid);

        monitor.ActiveCamera = camera;

        EnsureComp<ActiveSurveillanceCameraMonitorComponent>(uid); // Goobstation

        UpdateUserInterface(uid, monitor);
    }

    // Switches the camera's viewers over to this new given camera.
    private void SwitchCamera(EntityUid uid, EntityUid camera, SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor)
            || monitor.ActiveCamera == null)
        {
            return;
        }

        _surveillanceCameras.SwitchActiveViewers(monitor.ActiveCamera.Value, camera, monitor.Viewers, uid);

        monitor.ActiveCamera = camera;

        UpdateUserInterface(uid, monitor);
    }

    private void TrySwitchCameraByAddress(EntityUid uid, string address,
        SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor)) // Goobstation - removed extra checks since no more active subnet
        {
            return;
        }

        // Goobstation start
        foreach (var subnetAddress in monitor.KnownSubnets.Values)
        {
            var payload = new NetworkPayload()
            {
                {DeviceNetworkConstants.Command, SurveillanceCameraSystem.CameraConnectMessage},
                {SurveillanceCameraSystem.CameraAddressData, address}
            };

            monitor.NextCameraAddress = address;
            _deviceNetworkSystem.QueuePacket(uid, subnetAddress, payload);
        }
        // Goobstation end
    }

    // Attempts to switch over the current viewed camera on this monitor
    // to the new camera.
    private void TrySwitchCameraByUid(EntityUid uid, EntityUid newCamera, SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor))
        {
            return;
        }

        if (monitor.ActiveCamera == null)
        {
            SetCamera(uid, newCamera, monitor);
        }
        else
        {
            SwitchCamera(uid, newCamera, monitor);
        }
    }

    private void RemoveActiveCamera(EntityUid uid, SurveillanceCameraMonitorComponent? monitor = null)
    {
        if (!Resolve(uid, ref monitor)
            || monitor.ActiveCamera == null)
        {
            return;
        }

        _surveillanceCameras.RemoveActiveViewers(monitor.ActiveCamera.Value, monitor.Viewers, uid);

        UpdateUserInterface(uid, monitor);
    }

    // This is public primarily because it might be useful to have the ability to
    // have this component added to any entity, and have them open the BUI (somehow).
    public void AfterOpenUserInterface(EntityUid uid, EntityUid player, SurveillanceCameraMonitorComponent? monitor = null, ActorComponent? actor = null)
    {
        if (!Resolve(uid, ref monitor)
            || !Resolve(player, ref actor))
        {
            return;
        }

        AddViewer(uid, player);
    }

    private void UpdateUserInterface(EntityUid uid, SurveillanceCameraMonitorComponent? monitor = null, EntityUid? player = null)
    {
        if (!Resolve(uid, ref monitor))
        {
            return;
        }


        var state = new SurveillanceCameraMonitorUiState(GetNetEntity(monitor.ActiveCamera), monitor.ActiveCameraAddress, monitor.KnownCameras); // Goobstation
        _userInterface.SetUiState(uid, SurveillanceCameraMonitorUiKey.Key, state);
    }
}
