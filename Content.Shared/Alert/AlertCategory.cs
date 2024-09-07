namespace Content.Shared.Alert;

/// <summary>
/// Every category of alert. Corresponds to category field in alert prototypes defined in YML
/// </summary>
public enum AlertCategory
{
    Pressure,
    Temperature,
    Breathing,
    Buckled,
    Health,
    Mood,
    Internals,
    Stamina,
    Piloting,
    Hunger,
    Thirst,
    Toxins,
    Battery
}
