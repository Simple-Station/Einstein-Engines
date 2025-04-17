using Content.Shared.Chemistry.Components;

namespace Content.Shared._Goobstation.Chemistry.Hypospray;

[RegisterComponent]
public sealed partial class SolutionCartridgeComponent : Component
{
    [DataField]
    public string TargetSolution = "default";

    [DataField(required: true)]
    public Solution Solution;
}
