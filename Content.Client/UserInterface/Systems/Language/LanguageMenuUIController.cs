using Content.Client.Language;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Shared.Input;
using Content.Shared.Language.Events;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BaseButton;
using JetBrains.Annotations;

namespace Content.Client.UserInterface.Systems.Language;

[UsedImplicitly]
public sealed class LanguageMenuUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    public LanguageMenuWindow? LanguageWindow;
    private MenuButton? LanguageButton => UIManager.GetActiveUIWidgetOrNull<MenuBar.Widgets.GameTopMenuBar>()?.LanguageButton;

    public override void Initialize()
    {
        SubscribeNetworkEvent((LanguagesUpdatedMessage message, EntitySessionEventArgs _) =>
            LanguageWindow?.UpdateState(message.CurrentLanguage, message.Spoken));
    }

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(LanguageWindow == null);

        LanguageWindow = UIManager.CreateWindow<LanguageMenuWindow>();
        LayoutContainer.SetAnchorPreset(LanguageWindow, LayoutContainer.LayoutPreset.CenterTop);

        CommandBinds.Builder.Bind(ContentKeyFunctions.OpenLanguageMenu,
            InputCmdHandler.FromDelegate(_ => ToggleWindow())).Register<LanguageMenuUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        if (LanguageWindow != null)
        {
            LanguageWindow.Dispose();
            LanguageWindow = null;
        }

        CommandBinds.Unregister<LanguageMenuUIController>();
    }

    public void UnloadButton()
    {
        if (LanguageButton == null)
            return;

        LanguageButton.OnPressed -= LanguageButtonPressed;
    }

    public void LoadButton()
    {
        if (LanguageButton == null)
            return;

        LanguageButton.OnPressed += LanguageButtonPressed;

        if (LanguageWindow == null)
            return;

        LanguageWindow.OnClose += () => LanguageButton.Pressed = false;
        LanguageWindow.OnOpen += () => LanguageButton.Pressed = true;
    }

    private void LanguageButtonPressed(ButtonEventArgs args)
    {
        ToggleWindow();
    }

    private void ToggleWindow()
    {
        if (LanguageWindow == null)
            return;

        if (LanguageButton != null)
            LanguageButton.SetClickPressed(!LanguageWindow.IsOpen);

        if (LanguageWindow.IsOpen)
            LanguageWindow.Close();
        else
            LanguageWindow.Open();
    }
}
