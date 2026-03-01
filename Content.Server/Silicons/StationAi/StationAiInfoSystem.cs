using Content.Shared.Silicons.StationAi;
using Robust.Server.GameObjects;
using Content.Server.Actions;
using Robust.Shared.Player;
using Content.Server.AlertLevel;
using Content.Server.Station.Systems;
using Content.Shared.Robotics.Components;
using Content.Shared.Robotics;

namespace Content.Server.Silicons.StationAi;

public sealed partial class StationAiInfoSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationAiInfoComponent, BoundUIOpenedEvent>(OnWindowOpen);
        SubscribeLocalEvent<StationAiInfoComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StationAiInfoComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<StationAiInfoComponent, StationAiInfoActionEvent>(StationAiInfoAction);
        SubscribeLocalEvent<StationRenamedEvent>(OnStationRenamed);
        SubscribeLocalEvent<AlertLevelChangedEvent>(OnAlertLevelChanged);

        Subs.BuiEvents<StationAiInfoComponent>(StationAiInfoUiKey.Key, subs =>
        {
            subs.Event<RoboticsControlOpenUiMessage>(OnRoboticsControlOpen);
        });
    }

    private void OnWindowOpen(Entity<StationAiInfoComponent> ent, ref BoundUIOpenedEvent args)
    {
        if (!StationAiInfoUiKey.Key.Equals(args.UiKey))
            return;

        UpdateAllPdaUisOnStation();
    }

    private void OnRoboticsControlOpen(Entity<StationAiInfoComponent> ent, ref RoboticsControlOpenUiMessage args)
    {
        var query = EntityQueryEnumerator<RoboticsConsoleComponent>();
        while (query.MoveNext(out var consoleUid, out var _))
        {
            _uiSystem.TryOpenUi(consoleUid, RoboticsConsoleUiKey.Key, args.Actor);
            break;
        }
    }

    private void OnMapInit(EntityUid uid, StationAiInfoComponent component, MapInitEvent args)
    {
        _action.AddAction(uid, ref component.ActionEntity, component.Action);
    }

    private void OnShutdown(EntityUid uid, StationAiInfoComponent component, ComponentShutdown args)
    {
        _action.RemoveAction(uid, component.ActionEntity);
    }

    private void StationAiInfoAction(EntityUid uid, StationAiInfoComponent comp, StationAiInfoActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        _uiSystem.TryOpenUi(uid, StationAiInfoUiKey.Key, actor.Owner);

        args.Handled = true;
    }

    private void OnStationRenamed(StationRenamedEvent ev)
    {
        UpdateAllPdaUisOnStation();
    }

    private void OnAlertLevelChanged(AlertLevelChangedEvent args)
    {
        UpdateAllPdaUisOnStation();
    }

    private void UpdateAllPdaUisOnStation()
    {
        var query = AllEntityQuery<StationAiInfoComponent>();
        while (query.MoveNext(out var ent, out var comp))
        {
            UpdateWindow(ent, comp);
        }
    }
    public void UpdateWindow(EntityUid uid, StationAiInfoComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        if (!_uiSystem.HasUi(uid, StationAiInfoUiKey.Key))
            return;
        var aIName = _entity.GetComponent<MetaDataComponent>(uid).EntityName;

        UpdateStationName(uid, component);
        UpdateAlertLevel(uid, component);

        var state = new StationAiInfoUpdateState(
            aIName,
            component.StationName,
            component.StationAlertLevel,
            component.StationAlertColor
            );

        _uiSystem.SetUiState(uid, StationAiInfoUiKey.Key, state);
    }

    private void UpdateStationName(EntityUid uid, StationAiInfoComponent component)
    {
        var station = _station.GetOwningStation(uid);
        component.StationName = station is null ? null : Name(station.Value);
    }

    private void UpdateAlertLevel(EntityUid uid, StationAiInfoComponent component)
    {
        var station = _station.GetOwningStation(uid);
        if (!TryComp(station, out AlertLevelComponent? alertComp) ||
        alertComp.AlertLevels == null)
            return;
        component.StationAlertLevel = alertComp.CurrentLevel;
        if (alertComp.AlertLevels.Levels.TryGetValue(alertComp.CurrentLevel, out var details))
            component.StationAlertColor = details.Color;
    }
}
