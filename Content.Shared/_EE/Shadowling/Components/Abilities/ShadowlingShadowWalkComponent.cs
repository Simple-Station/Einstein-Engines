using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for Shadow Walk ability. Will also be used on Lesser Shadowlings.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingShadowWalkComponent : Component
{
    public string? ActionRapidRehatch = "ActionShadowWalk";

    [DataField]
    public bool IsActive;

    [DataField]
    public float WalkSpeedModifier = 1.5f;

    [DataField]
    public float RunSpeedModifier = 1.5f;

    [DataField]
    public float TimeUntilDeactivation = 10f;

    [DataField]
    public float Timer;
}
