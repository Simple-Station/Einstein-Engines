using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
/// I have no fucking idea what I'm doing///
namespace Content.Shared.Medical;

/// <summary>
/// This is used for penlights; a tool that med uses
/// to check for eye damages.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class PenLightComponent : Component
{
    /// <summary>
    /// Cooldown Time, exams take a bit
    /// </summary>
    [AutoPausedField]
    public TimeSpan? NextExamTime;

    /// <summary>
    /// The min time between exams
    /// </summary>
    [DataField]
    public TimeSpan ExamDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// How long the doafter for the exam takes
    /// </summary>
    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(3);

}

[Serializable, NetSerializable]
public sealed partial class PenLightDoAfterEvent : SimpleDoAfterEvent
{

}