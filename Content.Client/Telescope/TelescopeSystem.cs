using System.Numerics;
using Content.Client.Viewport;
using Content.Shared.CCVar;
using Content.Shared.Telescope;
using Content.Shared.Input;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Timing;

namespace Content.Client.Telescope;

public sealed class TelescopeSystem : SharedTelescopeSystem
{
    [Dependency] private readonly InputSystem _inputSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private ScalingViewport? _viewport;
    private bool _holdLookUp;
    private bool _toggled;

    public override void Initialize()
    {
        base.Initialize();

        _cfg.OnValueChanged(CCVars.HoldLookUp,
            val =>
            {
                var input = val ? null : InputCmdHandler.FromDelegate(_ => _toggled = !_toggled);
                _input.SetInputCommand(ContentKeyFunctions.LookUp, input);
                _holdLookUp = val;
                _toggled = false;
            },
            true);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_timing.ApplyingState
            || !_timing.IsFirstTimePredicted
            || !_input.MouseScreenPosition.IsValid)
            return;

        var player = _player.LocalEntity;

        var telescope = GetRightTelescope(player);

        if (telescope == null)
        {
            _toggled = false;
            return;
        }

        if (!TryComp<EyeComponent>(player, out var eye))
            return;

        var offset = Vector2.Zero;

        if (_holdLookUp)
        {
            if (_inputSystem.CmdStates.GetState(ContentKeyFunctions.LookUp) != BoundKeyState.Down)
            {
                RaiseEvent(offset);
                return;
            }
        }
        else if (!_toggled)
        {
            RaiseEvent(offset);
            return;
        }

        var mousePos = _input.MouseScreenPosition;

        if (_uiManager.MouseGetControl(mousePos) as ScalingViewport is { } viewport)
            _viewport = viewport;

        if (_viewport == null)
            return;

        var centerPos = _eyeManager.WorldToScreen(eye.Eye.Position.Position + eye.Offset);

        var diff = mousePos.Position - centerPos;
        var len = diff.Length();

        var size = _viewport.PixelSize;

        var maxLength = Math.Min(size.X, size.Y) * 0.4f;
        var minLength = maxLength * 0.2f;

        if (len > maxLength)
        {
            diff *= maxLength / len;
            len = maxLength;
        }

        var divisor = maxLength * telescope.Divisor;

        if (len > minLength)
        {
            diff -= diff * minLength / len;
            offset = new Vector2(diff.X / divisor, -diff.Y / divisor);
            offset = new Angle(-eye.Rotation.Theta).RotateVec(offset);
        }

        RaiseEvent(offset);
    }

    private void RaiseEvent(Vector2 offset)
    {
        RaisePredictiveEvent(new EyeOffsetChangedEvent
        {
            Offset = offset
        });
    }
}
