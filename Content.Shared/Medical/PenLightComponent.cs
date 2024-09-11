using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
namespace Content.Shared.Medical;

/// <summary>
///     This for penlights; a tool used to check for eye damage.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class PenLightComponent : Component
{

    /// <summary>
    ///     Cooldown Time, exams take a bit
    /// </summary>
    [AutoPausedField]
    public TimeSpan? NextExamTime;

    /// <summary>
    ///     The min time between exams
    /// </summary>
    [DataField]
    public TimeSpan ExamDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    ///     How long the doafter for the exam takes
    /// </summary>
    [DataField(required: true)]
    public float ExamSpeed { get; set; }

}

[Serializable, NetSerializable]
public sealed partial class PenLightDoAfterEvent : SimpleDoAfterEvent { }