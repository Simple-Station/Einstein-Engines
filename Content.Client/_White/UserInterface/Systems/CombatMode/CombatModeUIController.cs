using Content.Client._White.UserInterface.Systems.CombatMode.Widgets;
using Content.Client.CombatMode;
using Content.Client.Gameplay;
using Content.Shared.CombatMode;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client._White.UserInterface.Systems.CombatMode;

public sealed class CombatModeUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<CombatModeSystem>
{
    private CombatModeComponent? _combatModeComponent;

    private CombatModeGui? CombatModGui => UIManager.GetActiveUIWidgetOrNull<CombatModeGui>();

    public void OnSystemLoaded(CombatModeSystem system)
    {
        system.LocalPlayerCombatModeUpdated += OnCombatModeUpdated;
        system.LocalPlayerCombatModeAdded += OnCombatModeAdded;
        system.LocalPlayerCombatModeRemoved += OnCombatModeRemoved;
    }

    public void OnSystemUnloaded(CombatModeSystem system)
    {
        system.LocalPlayerCombatModeUpdated -= OnCombatModeUpdated;
        system.LocalPlayerCombatModeAdded -= OnCombatModeAdded;
        system.LocalPlayerCombatModeRemoved -= OnCombatModeRemoved;
    }

    public void OnStateEntered(GameplayState state)
    {
        if (CombatModGui != null)
            CombatModGui.Visible = _combatModeComponent is { Enable: true, };
    }

    private void OnCombatModeUpdated(bool inCombatMode)
    {
        CombatModGui?.OnCombatModeUpdated(inCombatMode);
    }

    private void OnCombatModeAdded(CombatModeComponent component)
    {
        if (CombatModGui != null)
            CombatModGui.Visible = component.Enable;

        _combatModeComponent = component;
        OnCombatModeUpdated(component.IsInCombatMode);
    }

    private void OnCombatModeRemoved()
    {
        if (CombatModGui != null)
            CombatModGui.Visible = false;

        _combatModeComponent = null;
    }

    public void ToggleCombatMode() => EntityManager.RaisePredictiveEvent(new ToggleCombatModeRequestEvent());
}
