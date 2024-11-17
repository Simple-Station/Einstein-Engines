namespace Content.Shared.TimeCycle;

[RegisterComponent]
public sealed partial class TimeCycleComponent : Component
{
    // Delayed time, before minute have been passed
    public TimeSpan? DelayTime;

    [DataField]
    public bool SpeedUp;

    [DataField]
    public bool Paused;

    [DataField]
    public TimeSpan MinuteDuration { get; set; } = TimeSpan.FromSeconds(4);

    [DataField]
    public TimeSpan SpeedUpMinuteDuration { get; set; } = TimeSpan.FromMilliseconds(10);

    // NOTE: Default time should be is noon
    [DataField]
    public TimeSpan CurrentTime { get; set; } = TimeSpan.FromHours(12);

    [DataField]
    public string PalettePrototype = "DefaultTimeCycle";
}
