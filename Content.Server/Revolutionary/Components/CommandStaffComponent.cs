using Content.Server.GameTicking.Rules;

namespace Content.Server.Revolutionary.Components;

/// <summary>
///     Component for tracking if someone is a Head of Staff.
/// </summary>
[RegisterComponent, Access(typeof(RevolutionaryRuleSystem))]
public sealed partial class CommandStaffComponent : Component
{
    public float PsionicBonusModifier = 1;
    public float PsionicBonusOffset = 0.25f;
}

//TODO this should probably be on a mind role, not the mob
