namespace Content.Shared.TimeCycle;

[RegisterComponent]
public sealed partial class TimeCycleComponent : Component
{
    // Delayed time, before minute have been passed
    public TimeSpan? DelayTime;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool SpeedUp = false;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool Paused = false;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string Palette = "DefaultTimeCycle";

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan MinuteLength { get; set; } = TimeSpan.FromSeconds(4);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan SpeedUpMinuteLength { get; set; } = TimeSpan.FromMilliseconds(10);

    // NOTE: Default time should be is noon
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan CurrentTime { get; set; } = TimeSpan.FromHours(12);
}
