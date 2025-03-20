using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Chemistry.SolutionCartridge;

[RegisterComponent, NetworkedComponent]
public sealed partial class SolutionCartridgeComponent : Component
{
    [DataField]
    public string TargetSolution = "default";

    [DataField(required: true)]
    public Solution Solution;
}
