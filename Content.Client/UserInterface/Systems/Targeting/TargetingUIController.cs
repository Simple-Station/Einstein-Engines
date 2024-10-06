/*using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.UserInterface.Systems.Targeting.Widgets;
using Content.Shared.Targeting;
using Content.Shared.Targeting.Events;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Utility;
using Robust.Client.Player;

namespace Content.Client.UserInterface.Systems.Targeting;

public sealed class TargetingUIController : UIController
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IEntityNetworkManager _net = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private SpriteSystem _spriteSystem = default!;
    private TargetBodyPart _currentTarget = TargetBodyPart.Torso; // Default to torso

    public override void Initialize()
    {
        base.Initialize();
        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
    }

    private void OnScreenLoad()
    {
        _spriteSystem = _entManager.System<SpriteSystem>();
    }

    public void CycleTarget(TargetingControl control)
    {
        if (_playerManager.LocalEntity is not { } user)
            return;

        var player = _entManager.GetNetEntity(user);
        _currentTarget = (TargetBodyPart) (((int) _currentTarget + 1) % Enum.GetValues(typeof(TargetBodyPart)).Length);
        var msg = new TargetChangeEvent(player, _currentTarget);
        _net.SendSystemNetworkMessage(msg);
        UpdateVisuals(control);
    }

    public void UpdateVisuals(TargetingControl control)
    {
        var stateName = $"doll-{_currentTarget.ToString().ToLowerInvariant()}";
        var spriteSpecifier = new SpriteSpecifier.Rsi(new ResPath("/Textures/Interface/Targeting/bodydoll.rsi"), stateName);
        control.SetTargetImage(_spriteSystem.Frame0(spriteSpecifier));
    }


}*/