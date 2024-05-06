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
    public LanguageMenuWindow? _languageWindow;
    private MenuButton? LanguageButton => UIManager.GetActiveUIWidgetOrNull<MenuBar.Widgets.GameTopMenuBar>()?.LanguageButton;

    public override void Initialize()
    {
        SubscribeNetworkEvent((LanguagesUpdatedMessage message, EntitySessionEventArgs args) => _languageWindow?.UpdateState(message.CurrentLanguage, message.Spoken));
    }

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_languageWindow == null);

        _languageWindow = UIManager.CreateWindow<LanguageMenuWindow>();
        LayoutContainer.SetAnchorPreset(_languageWindow, LayoutContainer.LayoutPreset.CenterTop);

        CommandBinds.Builder.Bind(ContentKeyFunctions.OpenLanguageMenu, InputCmdHandler.FromDelegate(_ => ToggleWindow())).Register<LanguageMenuUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        if (_languageWindow != null)
        {
            _languageWindow.Dispose();
            _languageWindow = null;
        }

        CommandBinds.Unregister<LanguageMenuUIController>();
    }

    public void UnloadButton()
    {
        if (LanguageButton == null)
        {
            return;
        }

        LanguageButton.OnPressed -= LanguageButtonPressed;
    }

    public void LoadButton()
    {
        if (LanguageButton == null)
        {
            return;
        }

        LanguageButton.OnPressed += LanguageButtonPressed;

        if (_languageWindow == null)
        {
            return;
        }

        _languageWindow.OnClose += DeactivateButton;
        _languageWindow.OnOpen += ActivateButton;
    }

    private void DeactivateButton() => LanguageButton!.Pressed = false;
    private void ActivateButton() => LanguageButton!.Pressed = true;

    private void LanguageButtonPressed(ButtonEventArgs args)
    {
        ToggleWindow();
    }

    private void CloseWindow()
    {
        _languageWindow?.Close();
    }

    private void ToggleWindow()
    {
        if (_languageWindow == null)
            return;

        if (LanguageButton != null)
        {
            LanguageButton.SetClickPressed(!_languageWindow.IsOpen);
        }

        if (_languageWindow.IsOpen)
        {
            CloseWindow();
        }
        else
        {
            _languageWindow.Open();
        }
    }
}
