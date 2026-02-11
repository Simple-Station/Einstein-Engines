using Content.Client._White.Bark;
using Content.Shared._White.Bark;
using Content.Shared.Humanoid;
using Robust.Client.UserInterface.Controls;
using Range = Robust.Client.UserInterface.Controls.Range;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Content.Client.Lobby.UI;


public partial class HumanoidProfileEditor
{
    private List<BarkVoicePrototype> _barkList = [];

    private BarkVoicePrototype? SelectedVoice =>
        VoiceBarkButton.SelectedId >= 0 &&
        VoiceBarkButton.SelectedId < _barkList.Count
            ? _barkList[VoiceBarkButton.SelectedId]
            : null;

    public void InitializeBark()
    {
        BarkPitchSlider.OnReleased += BarkPitchSliderValueChanged;
        BarkPitchVarianceSlider.OnReleased += BarkPitchVarianceSliderValueChanged;
        BarkPauseSlider.OnReleased += BarkPauseSliderValueChanged;

        VoiceBarkButton.OnItemSelected += VoiceBarkButtonItemSelected;
        VoiceBarkPlayButton.OnPressed += VoiceBarkPlayButtonPressed;
    }

    public void UpdateBarksControl()
    {
        VoiceBarkButton.Clear();
        if (Profile is null)
            return;

        _barkList = _entManager.System<BarkSystem>().GetVoiceList(Profile);
        if (_barkList.Count == 0)
        {
            SetBark(SharedHumanoidAppearanceSystem.DefaultBarkVoice, Profile.BarkSettings);
            return;
        }

        var selectedId = -1;

        for (var i = 0; i < _barkList.Count; i++)
        {
            var voice = _barkList[i];
            if (voice.ID == Profile.BarkVoice)
                selectedId = i;

            var name = Loc.GetString($"bark-{voice.ID.ToLower()}");
            VoiceBarkButton.AddItem(name, i);
        }

        if (selectedId == -1)
        {
            selectedId = 0;
            SetBark(_barkList[selectedId].ID, Profile.BarkSettings);
        }

        VoiceBarkButton.SelectId(selectedId);
        UpdateSliderValues();
    }

    private void SetBark(string proto, BarkPercentageApplyData settings){
        Profile = Profile?.WithBarkVoice(proto, settings);
        IsDirty = true;
        VoiceBarkPlayButtonPressed(default!);
    }

    private void VoiceBarkPlayButtonPressed(BaseButton.ButtonEventArgs obj)
    {
        if(Profile is null)
            return;

        _entManager.System<BarkPreviewSystem>()
            .PlayGlobal(Profile.BarkVoice, "Привет мир!", Profile.BarkSettings);
    }

    private void VoiceBarkButtonItemSelected(OptionButton.ItemSelectedEventArgs selected)
    {
        if(Profile is null || SelectedVoice is null)
            return;

        VoiceBarkButton.SelectId(selected.Id);
        SetBark(SelectedVoice.ID, Profile.BarkSettings);
    }

    private void UpdateSliderValues()
    {
        if(Profile is null)
            return;

        BarkPauseSlider.Value = Profile.BarkSettings.Pause;
        BarkPitchSlider.Value = Profile.BarkSettings.Pitch;
        BarkPitchVarianceSlider.Value = Profile.BarkSettings.PitchVariance;
    }

    private void BarkPauseSliderValueChanged(Range range)
    {
        if(Profile is null)
            return;

        SetBark(
            Profile.BarkVoice,
            new()
        {
            Pause = (byte)range.Value,
            Pitch = Profile.BarkSettings.Pitch,
            Volume = Profile.BarkSettings.Volume,
            PitchVariance = Profile.BarkSettings.PitchVariance
        });
    }

    private void BarkPitchVarianceSliderValueChanged(Range range)
    {
        if(Profile is null)
            return;

        SetBark(
            Profile.BarkVoice,
            new()
            {
                Pause = Profile.BarkSettings.Pause,
                Pitch = Profile.BarkSettings.Pitch,
                Volume = Profile.BarkSettings.Volume,
                PitchVariance = (byte)range.Value
            });
    }

    private void BarkPitchSliderValueChanged(Range range)
    {
        if(Profile is null)
            return;

        SetBark(
            Profile.BarkVoice,
            new()
            {
                Pause = Profile.BarkSettings.Pause,
                Pitch = (byte)range.Value,
                Volume = Profile.BarkSettings.Volume,
                PitchVariance = Profile.BarkSettings.PitchVariance
            });
    }
}
