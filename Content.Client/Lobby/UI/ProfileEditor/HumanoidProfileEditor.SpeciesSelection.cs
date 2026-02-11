using Content.Client._White.UserInterface.Windows;
using Content.Shared.Humanoid.Prototypes;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;


namespace Content.Client.Lobby.UI;

// WWDP PARTIAL CLASS
public sealed partial class HumanoidProfileEditor
{
    private SpeciesSelectWindow? _currentWindow;

    public void InitializeSpeciesSelection()
    {
        OpenSpeciesWindow.OnPressed += OpenSpeciesWindowPressed;
    }

    private void OpenSpeciesWindowPressed(BaseButton.ButtonEventArgs obj)
    {
        if(Profile is null)
            return;

        OpenSpeciesWindow.Disabled = true;
        _currentWindow = UserInterfaceManager.CreateWindow<SpeciesSelectWindow>();
        _currentWindow.Initialize(Profile);
        _currentWindow.OnClose += CurrentWindowClosed;
        _currentWindow.OnSpeciesSelected += OnSpeciesSelected;
        _currentWindow.OpenCentered();
    }

    private void OnSpeciesSelected(ProtoId<SpeciesPrototype> proto)
    {
        SetSpecies(proto);
        UpdateHairPickers();
        OnSkinColorOnValueChanged();
        UpdateCustomSpecieNameEdit();
        UpdateHeightWidthSliders();
        _currentWindow?.Close();
    }

    private void CurrentWindowClosed()
    {
        if(_currentWindow == null)
            return;
        _currentWindow.OnClose -= CurrentWindowClosed;
        _currentWindow.OnSpeciesSelected -= OnSpeciesSelected;
        OpenSpeciesWindow.Disabled = false;
    }
}
