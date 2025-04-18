using System.Numerics;
using Content.Server.UserInterface;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Content.Shared.PowerCell;
using Content.Shared.Movement.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Shuttles.Systems;


public sealed partial class RadarConsoleSystem : SharedRadarConsoleSystem
{
    public void RefreshIFFState()
    {
        var query = AllEntityQuery<RadarConsoleComponent>();
        while (query.MoveNext(out var uid, out var console))
        {
            if (console.LastUpdatedState == null || console.LastUpdatedState.IFFState == null)
            {
                continue;
            }

            console.LastUpdatedState.IFFState.Turrets = _console.GetAllTurrets(uid);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<RadarConsoleComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var console, out var transform))
        {
            if (console.LastUpdatedState == null || !_uiSystem.IsUiOpen(uid, RadarConsoleUiKey.Key))
            {
                continue;
            }

            var turrets = console.LastUpdatedState.IFFState?.Turrets;
            var iffState = _console.GetIFFState(uid, transform, turrets);
            var state = new NavBoundUserInterfaceState(console.LastUpdatedState);
            state.IFFState = iffState;

            if (state.DirtyFlags < NavBoundUserInterfaceState.StateDirtyFlags.IFF)
            {
                state.DirtyFlags |= NavBoundUserInterfaceState.StateDirtyFlags.IFF;
            }
            else if (state.DirtyFlags > NavBoundUserInterfaceState.StateDirtyFlags.IFF)
            {
                state.DirtyFlags = NavBoundUserInterfaceState.StateDirtyFlags.IFF;
            }

            console.LastUpdatedState = state;
            _uiSystem.SetUiState(uid, RadarConsoleUiKey.Key, state);
        }
    }
}
