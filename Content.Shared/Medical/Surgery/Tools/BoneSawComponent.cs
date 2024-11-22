using Robust.Shared.GameStates;

namespace Content.Shared.Medical.Surgery.Tools;

[RegisterComponent, NetworkedComponent]
public sealed partial class BoneSawComponent : Component, ISurgeryToolComponent
{
    public string ToolName => "a bone saw";
    public bool? Used { get; set; } = null;
}