namespace Content.Shared.Traits.Assorted.Components;

[RegisterComponent]
public sealed partial class CyberEyesComponent : Component
{
    /// <summary>
    ///     The text that will appear when someone with the CyberEyes component is examined at close range
    /// </summary>
    [DataField]
    public string CyberEyesExaminationText = "examine-cybereyes-message";
}
