using System.Threading;
using Content.Shared.PointCannons;
using Timer = Robust.Shared.Timing.Timer;
using JetBrains.Annotations;
using System.Numerics;
using Content.Client._Crescent.PointCannons;
using Robust.Client.GameObjects;
using Content.Shared.Weapons.Ranged.Events;
using OpenToolkit.GraphicsLibraryFramework;
using Content.Client.Weapons.Ranged.Systems;

namespace Content.Client._Crescent.PointCannons;

[UsedImplicitly]
public sealed class TargetingConsoleBoundUserInterface : BoundUserInterface
{
    private IEntityManager _entMan;
    private TransformSystem _formSys;

    private TargetingConsoleWindow? _window;
    private bool _isFiring;
    private Vector2 _coords;
    private CancellationTokenSource _updTimerTok = new();
    private List<NetEntity>? _controlled;

    public TargetingConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _entMan = IoCManager.Resolve<IEntityManager>();
        _formSys = _entMan.System<TransformSystem>();
        Timer.SpawnRepeating(100, Update, _updTimerTok.Token);
    }

    private void Update()
    {
        if (_isFiring)
            SendMessage(new TargetingConsoleFireMessage(_coords));

        if (_controlled == null || _window == null)
            return;

        var query = _entMan.EntityQueryEnumerator<PointCannonComponent>();
        List<(int, int)> ammoValues = new();
        while (query.MoveNext(out var uid, out var _))
        {
            if (_controlled.Contains(_entMan.GetNetEntity(uid)))
            {
                GetAmmoCountEvent ammoEv = new();
                _entMan.EventBus.RaiseLocalEvent(uid, ref ammoEv);
                ammoValues.Add((ammoEv.Count, ammoEv.Capacity));
            }
        }
        _window.UpdateAmmoStatus(ammoValues);
    }

    protected override void Open()
    {
        base.Open();

        _window = new TargetingConsoleWindow();
        _window.OpenCentered();
        _window.OnClose += Close;

        _window.Radar.OnRadarClick += (coords) =>
        {
            _coords = _formSys.ToMapCoordinates(coords).Position;
            SendMessage(new TargetingConsoleFireMessage(_coords));
            _isFiring = true;
        };

        _window.Radar.OnRadarRelease += () =>
        {
            _isFiring = false;
        };

        _window.Radar.OnRadarMouseMove += (coords) =>
        {
            _coords = _formSys.ToMapCoordinates(coords).Position;
        };

        _window.OnCannonGroupChange += (groupName) =>
        {
            SendMessage(new TargetingConsoleGroupChangedMessage(groupName));
        };
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _updTimerTok.Cancel();
            _window?.Dispose();
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not TargetingConsoleBoundUserInterfaceState consoleState)
            return;

        _controlled = consoleState.ControlledCannons;
        _window?.UpdateState(consoleState);
    }
}
