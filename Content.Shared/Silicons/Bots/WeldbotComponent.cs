using Robust.Shared.Audio;

namespace Content.Shared.Silicons.Bots;

/// <summary>
/// Used by the server for NPC Weldbot welding.
/// Currently no clientside prediction done, only exists in shared for emag handling.
/// </summary>
[RegisterComponent]
[Access(typeof(WeldbotSystem))]
public sealed partial class WeldbotComponent : Component
{
    /// <summary>
    /// Sound played after welding a patient.
    /// </summary>
    [DataField("weldSound")]
    public SoundSpecifier WeldSound = new SoundPathSpecifier("/Audio/Items/welder2.ogg");

    public bool IsEmagged = false;
}
