using System.Numerics;
using Content.Server.Access.Systems;
using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Emp;
using Content.Server.Medical.CrewMonitoring;
using Content.Server.Popups;
using Content.Server.Station.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.DeviceNetwork;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.GameTicking;
using Content.Shared.Interaction;
using Content.Shared.Medical.SuitSensor;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Medical.SuitSensors;

public sealed class SuitSensorSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
    [Dependency] private readonly IdCardSystem _idCardSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly SingletonDeviceNetServerSystem _singletonServerSystem = default!;
    [Dependency] private readonly MobThresholdSystem _mobThresholdSystem = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;

    private readonly HashSet<Entity<SuitSensorComponent>> _wornSensors = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn);
        SubscribeLocalEvent<SuitSensorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SuitSensorComponent, ClothingGotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<SuitSensorComponent, ClothingGotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<SuitSensorComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<SuitSensorComponent, GetVerbsEvent<Verb>>(OnVerb);
        SubscribeLocalEvent<SuitSensorComponent, EntGotInsertedIntoContainerMessage>(OnInsert);
        SubscribeLocalEvent<SuitSensorComponent, EntGotRemovedFromContainerMessage>(OnRemove);
        SubscribeLocalEvent<SuitSensorComponent, EmpPulseEvent>(OnEmpPulse);
        SubscribeLocalEvent<SuitSensorComponent, EmpDisabledRemoved>(OnEmpFinished);
        SubscribeLocalEvent<SuitSensorComponent, SuitSensorChangeDoAfterEvent>(OnSuitSensorDoAfter);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _gameTiming.CurTime;
        foreach (var ent in _wornSensors)
        {
            var (uid, sensor) = ent;
            if (!TryComp(uid, out DeviceNetworkComponent? device)
                || device.TransmitFrequency is null
                || !Exists(sensor.User))
            {
                _wornSensors.Remove(ent); //Not a valid suit sensor array, cease all processing for it.
                continue;
            }

            // check if sensor is ready to update
            if (curTime < sensor.NextUpdate)
                continue;

            if (!CheckSensorAssignedStation(ent))
                continue;

            // TODO: This would cause imprecision at different tick rates.
            sensor.NextUpdate = curTime + sensor.UpdateRate;

            // get sensor status
            var status = GetSensorState(uid, sensor);
            if (status == null)
            {
                _wornSensors.Remove(ent); //Someone turned the suit off, cease all checking.
                continue;
            }

            //Retrieve active server address if the sensor isn't connected to a server
            if (sensor.ConnectedServer == null)
            {
                if (!_singletonServerSystem.TryGetActiveServerAddress<CrewMonitoringServerComponent>(sensor.StationId!.Value, out var address))
                    continue;

                sensor.ConnectedServer = address;
            }

            // Send it to the connected server
            var payload = SuitSensorToPacket(status);

            // Clear the connected server if its address isn't on the network
            if (!_deviceNetworkSystem.IsAddressPresent(device.DeviceNetId, sensor.ConnectedServer))
            {
                sensor.ConnectedServer = null;
                continue;
            }

            _deviceNetworkSystem.QueuePacket(uid, sensor.ConnectedServer, payload, device: device);
        }
    }

    /// <summary>
    /// Checks whether the sensor is assigned to a station or not
    /// and tries to assign an unassigned sensor to a station if it's currently on a grid
    /// </summary>
    /// <returns>True if the sensor is assigned to a station or assigning it was successful. False otherwise.</returns>
    private bool CheckSensorAssignedStation(Entity<SuitSensorComponent> ent)
    {
        var (uid, sensor) = ent;
        var xform = Transform(uid);
        if (!sensor.StationId.HasValue && xform.GridUid is null)
            return false;

        sensor.StationId = _stationSystem.GetOwningStation(uid, xform);
        return sensor.StationId.HasValue;
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent ev)
    {
        // If the player spawns in arrivals then the grid underneath them may not be appropriate.
        // in which case we'll just use the station spawn code told us they are attached to and set all of their
        // sensors.
        var sensorQuery = GetEntityQuery<SuitSensorComponent>();
        var xformQuery = GetEntityQuery<TransformComponent>();
        RecursiveSensor(ev.Mob, ev.Station, sensorQuery, xformQuery);
    }

    private void RecursiveSensor(EntityUid uid, EntityUid stationUid, EntityQuery<SuitSensorComponent> sensorQuery, EntityQuery<TransformComponent> xformQuery)
    {
        var xform = xformQuery.GetComponent(uid);
        var enumerator = xform.ChildEnumerator;

        while (enumerator.MoveNext(out var child))
        {
            if (sensorQuery.TryGetComponent(child, out var sensor))
            {
                sensor.StationId = stationUid;
            }

            RecursiveSensor(child, stationUid, sensorQuery, xformQuery);
        }
    }

    private void OnMapInit(EntityUid uid, SuitSensorComponent component, MapInitEvent args)
    {
        // Fallback
        component.StationId ??= _stationSystem.GetOwningStation(uid);

        // generate random mode
        if (component.RandomMode)
        {
            //make the sensor mode favor higher levels, except coords.
            var modesDist = new[]
            {
                SuitSensorMode.SensorOff,
                SuitSensorMode.SensorBinary, SuitSensorMode.SensorBinary,
                SuitSensorMode.SensorVitals, SuitSensorMode.SensorVitals, SuitSensorMode.SensorVitals,
                SuitSensorMode.SensorCords, SuitSensorMode.SensorCords
            };
            component.Mode = _random.Pick(modesDist);
        }
    }

    private void OnEquipped(Entity<SuitSensorComponent> ent, ref ClothingGotEquippedEvent args)
    {
        ent.Comp.User = args.Wearer;
        _wornSensors.Add(ent);
    }

    private void OnUnequipped(Entity<SuitSensorComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        ent.Comp.User = null;
        _wornSensors.Remove(ent);
    }

    private void OnExamine(EntityUid uid, SuitSensorComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        string msg;
        switch (component.Mode)
        {
            case SuitSensorMode.SensorOff:
                msg = "suit-sensor-examine-off";
                break;
            case SuitSensorMode.SensorBinary:
                msg = "suit-sensor-examine-binary";
                break;
            case SuitSensorMode.SensorVitals:
                msg = "suit-sensor-examine-vitals";
                break;
            case SuitSensorMode.SensorCords:
                msg = "suit-sensor-examine-cords";
                break;
            default:
                return;
        }

        args.PushMarkup(Loc.GetString(msg));
    }

    private void OnVerb(EntityUid uid, SuitSensorComponent component, GetVerbsEvent<Verb> args)
    {
        // check if user can change sensor
        if (component.ControlsLocked)
            return;

        // standard interaction checks
        if (!args.CanInteract || args.Hands == null)
            return;

        if (!_interactionSystem.InRangeUnobstructed(args.User, args.Target))
            return;

        // check if target is incapacitated (cuffed, dead, etc)
        if (component.User != null && args.User != component.User && _actionBlocker.CanInteract(component.User.Value, null))
            return;

        args.Verbs.UnionWith(new[]
        {
            CreateVerb(uid, component, args.User, SuitSensorMode.SensorOff),
            CreateVerb(uid, component, args.User, SuitSensorMode.SensorBinary),
            CreateVerb(uid, component, args.User, SuitSensorMode.SensorVitals),
            CreateVerb(uid, component, args.User, SuitSensorMode.SensorCords)
        });
    }

    private void OnInsert(EntityUid uid, SuitSensorComponent component, EntGotInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != component.ActivationContainer)
            return;

        component.User = args.Container.Owner;
        _wornSensors.Add((uid, component));
    }

    private void OnRemove(EntityUid uid, SuitSensorComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (args.Container.ID != component.ActivationContainer)
            return;

        component.User = null;
        _wornSensors.Remove((uid, component));
    }

    private void OnEmpPulse(EntityUid uid, SuitSensorComponent component, ref EmpPulseEvent args)
    {
        args.Affected = true;
        args.Disabled = true;

        component.PreviousMode = component.Mode;
        SetSensor((uid, component), SuitSensorMode.SensorOff, null);

        component.PreviousControlsLocked = component.ControlsLocked;
        component.ControlsLocked = true;
    }

    private void OnEmpFinished(EntityUid uid, SuitSensorComponent component, ref EmpDisabledRemoved args)
    {
        SetSensor((uid, component), component.PreviousMode, null);
        component.ControlsLocked = component.PreviousControlsLocked;
    }

    private Verb CreateVerb(EntityUid uid, SuitSensorComponent component, EntityUid userUid, SuitSensorMode mode)
    {
        return new Verb()
        {
            Text = GetModeName(mode),
            Disabled = component.Mode == mode,
            Priority = -(int) mode, // sort them in descending order
            Category = VerbCategory.SetSensor,
            Act = () => TrySetSensor((uid, component), mode, userUid)
        };
    }

    private string GetModeName(SuitSensorMode mode)
    {
        string name;
        switch (mode)
        {
            case SuitSensorMode.SensorOff:
                name = "suit-sensor-mode-off";
                break;
            case SuitSensorMode.SensorBinary:
                name = "suit-sensor-mode-binary";
                break;
            case SuitSensorMode.SensorVitals:
                name = "suit-sensor-mode-vitals";
                break;
            case SuitSensorMode.SensorCords:
                name = "suit-sensor-mode-cords";
                break;
            default:
                return "";
        }

        return Loc.GetString(name);
    }

    public void TrySetSensor(Entity<SuitSensorComponent> sensors, SuitSensorMode mode, EntityUid userUid)
    {
        var comp = sensors.Comp;

        if (!Resolve(sensors, ref comp))
            return;

        if (comp.User == null || userUid == comp.User)
            SetSensor(sensors, mode, userUid);
        else
        {
            var doAfterEvent = new SuitSensorChangeDoAfterEvent(mode);
            var doAfterArgs = new DoAfterArgs(EntityManager, userUid, comp.SensorsTime, doAfterEvent, sensors)
            {
                BreakOnMove = true,
                BreakOnDamage = true
            };

            _doAfterSystem.TryStartDoAfter(doAfterArgs);
        }
    }

    private void OnSuitSensorDoAfter(Entity<SuitSensorComponent> sensors, ref SuitSensorChangeDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        SetSensor(sensors, args.Mode, args.User);
    }

    public void SetSensor(Entity<SuitSensorComponent> sensors, SuitSensorMode mode, EntityUid? userUid = null)
    {
        var comp = sensors.Comp;

        comp.Mode = mode;
        if (mode == SuitSensorMode.SensorOff)
            _wornSensors.Remove(sensors);
        else if (comp.User != null)
            _wornSensors.Add(sensors);


        if (userUid != null)
        {
            var msg = Loc.GetString("suit-sensor-mode-state", ("mode", GetModeName(mode)));
            _popupSystem.PopupEntity(msg, sensors, userUid.Value);
        }
    }

    public SuitSensorStatus? GetSensorState(EntityUid uid, SuitSensorComponent? sensor = null, TransformComponent? transform = null)
    {
        if (!Resolve(uid, ref sensor, ref transform))
            return null;

        // check if sensor is enabled and worn by user
        if (sensor.Mode == SuitSensorMode.SensorOff || sensor.User == null || !HasComp<MobStateComponent>(sensor.User) || transform.GridUid == null)
            return null;

        // try to get mobs id from ID slot
        var userName = Loc.GetString("suit-sensor-component-unknown-name");
        var userJob = Loc.GetString("suit-sensor-component-unknown-job");
        var userJobIcon = "JobIconNoId";
        var userJobDepartments = new List<string>();

        if (_idCardSystem.TryFindIdCard(sensor.User.Value, out var card))
        {
            if (card.Comp.FullName != null)
                userName = card.Comp.FullName;
            if (card.Comp.LocalizedJobTitle != null)
                userJob = card.Comp.LocalizedJobTitle;
            userJobIcon = card.Comp.JobIcon;

            foreach (var department in card.Comp.JobDepartments)
                userJobDepartments.Add(Loc.GetString($"department-{department}"));
        }

        // get health mob state
        var isAlive = false;
        if (EntityManager.TryGetComponent(sensor.User.Value, out MobStateComponent? mobState))
            isAlive = !_mobStateSystem.IsDead(sensor.User.Value, mobState);

        // get mob total damage
        var totalDamage = 0;
        if (TryComp<DamageableComponent>(sensor.User.Value, out var damageable))
            totalDamage = damageable.TotalDamage.Int();

        // Get mob total damage crit threshold
        int? totalDamageThreshold = null;
        if (_mobThresholdSystem.TryGetThresholdForState(sensor.User.Value, MobState.Critical, out var critThreshold))
            totalDamageThreshold = critThreshold.Value.Int();

        // finally, form suit sensor status
        var status = new SuitSensorStatus(GetNetEntity(uid), userName, userJob, userJobIcon, userJobDepartments);
        switch (sensor.Mode)
        {
            case SuitSensorMode.SensorBinary:
                status.IsAlive = isAlive;
                break;
            case SuitSensorMode.SensorVitals:
                status.IsAlive = isAlive;
                status.TotalDamage = totalDamage;
                status.TotalDamageThreshold = totalDamageThreshold;
                break;
            case SuitSensorMode.SensorCords:
                status.IsAlive = isAlive;
                status.TotalDamage = totalDamage;
                status.TotalDamageThreshold = totalDamageThreshold;
                EntityCoordinates coordinates;
                var xformQuery = GetEntityQuery<TransformComponent>();

                if (transform.GridUid != null)
                {
                    coordinates = new EntityCoordinates(transform.GridUid.Value,
                        Vector2.Transform(_transform.GetWorldPosition(transform, xformQuery),
                            _transform.GetInvWorldMatrix(xformQuery.GetComponent(transform.GridUid.Value), xformQuery)));
                }
                else if (transform.MapUid != null)
                {
                    coordinates = new EntityCoordinates(transform.MapUid.Value,
                        _transform.GetWorldPosition(transform, xformQuery));
                }
                else
                {
                    coordinates = EntityCoordinates.Invalid;
                }

                status.Coordinates = GetNetCoordinates(coordinates);
                break;
        }

        return status;
    }

    /// <summary>
    ///     Serialize create a device network package from the suit sensors status.
    /// </summary>
    public NetworkPayload SuitSensorToPacket(SuitSensorStatus status)
    {
        var payload = new NetworkPayload()
        {
            [DeviceNetworkConstants.Command] = DeviceNetworkConstants.CmdUpdatedState,
            [SuitSensorConstants.NET_NAME] = status.Name,
            [SuitSensorConstants.NET_JOB] = status.Job,
            [SuitSensorConstants.NET_JOB_ICON] = status.JobIcon,
            [SuitSensorConstants.NET_JOB_DEPARTMENTS] = status.JobDepartments,
            [SuitSensorConstants.NET_IS_ALIVE] = status.IsAlive,
            [SuitSensorConstants.NET_SUIT_SENSOR_UID] = status.SuitSensorUid,
        };

        if (status.TotalDamage != null)
            payload.Add(SuitSensorConstants.NET_TOTAL_DAMAGE, status.TotalDamage);
        if (status.TotalDamageThreshold != null)
            payload.Add(SuitSensorConstants.NET_TOTAL_DAMAGE_THRESHOLD, status.TotalDamageThreshold);
        if (status.Coordinates != null)
            payload.Add(SuitSensorConstants.NET_COORDINATES, status.Coordinates);

        return payload;
    }

    /// <summary>
    ///     Try to create the suit sensors status from the device network message
    /// </summary>
    public SuitSensorStatus? PacketToSuitSensor(NetworkPayload payload)
    {
        // check command
        if (!payload.TryGetValue(DeviceNetworkConstants.Command, out string? command))
            return null;
        if (command != DeviceNetworkConstants.CmdUpdatedState)
            return null;

        // check name, job and alive
        if (!payload.TryGetValue(SuitSensorConstants.NET_NAME, out string? name)) return null;
        if (!payload.TryGetValue(SuitSensorConstants.NET_JOB, out string? job)) return null;
        if (!payload.TryGetValue(SuitSensorConstants.NET_JOB_ICON, out string? jobIcon)) return null;
        if (!payload.TryGetValue(SuitSensorConstants.NET_JOB_DEPARTMENTS, out List<string>? jobDepartments)) return null;
        if (!payload.TryGetValue(SuitSensorConstants.NET_IS_ALIVE, out bool? isAlive)) return null;
        if (!payload.TryGetValue(SuitSensorConstants.NET_SUIT_SENSOR_UID, out NetEntity suitSensorUid)) return null;

        // try get total damage and cords (optionals)
        payload.TryGetValue(SuitSensorConstants.NET_TOTAL_DAMAGE, out int? totalDamage);
        payload.TryGetValue(SuitSensorConstants.NET_TOTAL_DAMAGE_THRESHOLD, out int? totalDamageThreshold);
        payload.TryGetValue(SuitSensorConstants.NET_COORDINATES, out NetCoordinates? coords);

        var status = new SuitSensorStatus(suitSensorUid, name, job, jobIcon, jobDepartments)
        {
            IsAlive = isAlive.Value,
            TotalDamage = totalDamage,
            TotalDamageThreshold = totalDamageThreshold,
            Coordinates = coords,
        };
        return status;
    }
}
