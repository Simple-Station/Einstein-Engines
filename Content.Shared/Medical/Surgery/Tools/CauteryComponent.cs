using Robust.Shared.GameStates;

namespace Content.Shared.Medical.Surgery.Tools;

[RegisterComponent, NetworkedComponent]
public sealed partial class CauteryComponent : Component, ISurgeryToolComponent
{
    public string ToolName => "a cautery";
    public bool? Used { get; set; } = null;
}