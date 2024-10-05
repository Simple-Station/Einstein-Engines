using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.Targeting;

/// <summary>
/// Controls entity limb targeting for actions.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TargetingComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TargetBodyPart Target = TargetBodyPart.Torso;

    [DataField]
    public Dictionary<TargetBodyPart, float> TargetOdds = new()
    {
        { TargetBodyPart.Head, 0.1f },
        { TargetBodyPart.Torso, 0.4f },
        { TargetBodyPart.LeftArm, 0.125f },
        { TargetBodyPart.RightArm, 0.125f },
        { TargetBodyPart.LeftLeg, 0.125f },
        { TargetBodyPart.RightLeg, 0.125f }
    };
    /// <summary>
    /// What noise does the entity play when swapping targets?
    /// </summary>
    [DataField]
    public string SwapSound = "/Audio/Effects/toggleoncombat.ogg";
}