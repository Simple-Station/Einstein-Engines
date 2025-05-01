namespace Content.Shared.Actions.Events;
public sealed partial class PsionicInvisibilityPowerActionEvent : InstantActionEvent
{
    [DataField]
    public float PowerTimer = 30f;

    [DataField]
    public float MinGlimmer = 4f;

    [DataField]
    public float MaxGlimmer = 6f;
}
