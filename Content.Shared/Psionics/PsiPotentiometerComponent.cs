using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Psionics;

/// <summary>
///     This for penlights; a tool used to check for eye damage.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class PsiPotentiometerComponent : Component
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

    /// <summary>
    ///     Whether the Potentiometer is an innate ability and not an item. Such as a Metapsionic Pulse user.
    /// </summary>
    [DataField]
    public bool Innate;

    /// <summary>
    ///     The BUI configuration for the instrument.
    /// </summary>
    [DataField]
    public PrototypeData? PotentiometerUi;
}

[Serializable, NetSerializable]
public sealed partial class PsiPotentiometerDoAfterEvent : SimpleDoAfterEvent { }