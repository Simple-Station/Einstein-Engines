using Content.Shared.Popups;
using Robust.Shared.GameStates;

namespace Content.Shared.Changeling;


[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingInfectionComponent : Component
{
    [DataField]
    public TimeSpan FirstSymptoms = TimeSpan.Zero;

    [DataField]
    public TimeSpan KnockedOut = TimeSpan.Zero;

    [DataField]
    public TimeSpan FullyInfected = TimeSpan.Zero;

    [DataField]
    public TimeSpan EffectsTimer = TimeSpan.Zero;

    [DataField]
    public float EffectsTimerDelay = 20f;

    [DataField]
    public TimeSpan FirstSymptomsDelay = TimeSpan.FromSeconds(600);

    [DataField]
    public TimeSpan KnockedOutDelay = TimeSpan.FromSeconds(1200);

    [DataField]
    public TimeSpan FullyInfectedDelay = TimeSpan.FromSeconds(1320);

    [DataField]
    public float ScarySymptomChance = 0.1f;

    public enum InfectionState
    {
        None,
        FirstSymptoms,
        KnockedOut,
        FullyInfected
    }

    public List<string> SymptomMessages = new()
    {
        "changeling-convert-warning-1",
        "changeling-convert-warning-2",
        "changeling-convert-warning-3",
    };

    public List<string> EepyMessages = new()
    {
        "changeling-convert-eeped-1",
        "changeling-convert-eeped-2",
        "changeling-convert-eeped-3",
        "changeling-convert-eeped-4",
    };


    [DataField]
    public InfectionState CurrentState = InfectionState.None;

    // Whether the component has spawned and needs timer setup done
    public bool NeedsInitialization = false;

    /// <summary>
    ///     How much time (in hours) between progression states for the Changeling Infection.
    /// </summary>
    [DataField]
    public TimeSpan SymptomProgressionTime = TimeSpan.FromHours(24);

    [DataField]
    public string ConvertWarningThrowupText = "changeling-convert-warning-throwup";

    [DataField]
    public PopupType ConvertThrowupPopupType = PopupType.Medium;

    [DataField]
    public string ConvertWarningCollapseText = "changeling-convert-warning-collapse";

    [DataField]
    public PopupType ConvertCollapsePopupType = PopupType.Medium;

    [DataField]
    public float ConvertParalyzeTime = 5f;

    [DataField]
    public string ConvertWarningShakeText = "changeling-convert-warning-shake";

    [DataField]
    public PopupType ConvertShakePopupType = PopupType.Medium;

    [DataField]
    public float ConvertJitterTime = 5f;

    [DataField]
    public float ConvertJitterAmplitude = 10f;

    [DataField]
    public float ConvertJitterFrequency = 4f;

    [DataField]
    public string ConvertEepedText = "changeling-convert-eeped";

    [DataField]
    public PopupType ConvertEeepedPopupType = PopupType.LargeCaution;

    [DataField]
    public string ConvertEeepedShakeText = "changeling-convert-eeped-shake";

    [DataField]
    public PopupType ConvertEeepedShakePopupType = PopupType.Medium;

    [DataField]
    public string ConvertSkillIssue = "changeling-convert-skillissue";
}
