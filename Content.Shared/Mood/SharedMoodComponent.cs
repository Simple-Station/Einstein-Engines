namespace Content.Shared.Mood;

/// <summary>
///     This component exists solely to network CurrentMoodLevel, so that clients can make use of its value for math Prediction.
///     All mood logic is otherwise handled by the Server, and the client is not allowed to know the identity of its mood events.
/// </summary>
[RegisterComponent, AutoGenerateComponentState]
public sealed partial class NetMoodComponent : Component
{
    [DataField, AutoNetworkedField]
    public float CurrentMoodLevel;

    [DataField, AutoNetworkedField]
    public float NeutralMoodThreshold;
}