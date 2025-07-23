using Robust.Shared.GameStates;

namespace Content.Shared.Changeling;


/// <summary>
///     Component that indicates that a person's DNA has been absorbed by a changeling.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(AbsorbedSystem))]
public sealed partial class AbsorbedComponent : Component
{
    [DataField]
    public string ExamineText = "changeling-absorb-onexamine";
}
