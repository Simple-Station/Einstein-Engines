namespace Content.Server.Revolutionary.Components;

/// <summary>
///     Component for tracking if someone is a Head of Staff. Used for assigning traitors to kill heads and for revs to check if the heads died or not.
/// </summary>
[RegisterComponent]
public sealed partial class CommandStaffComponent : Component
{
    public float PsionicBonusModifier = 1;
    public float PsionicBonusOffset = 0.25f;
}

//TODO this should probably be on a mind role, not the mob
