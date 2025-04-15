using Content.Shared.PointCannons;

namespace Content.Server.PointCannons;

[RegisterComponent]
public sealed partial class TargetingConsoleComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<string, List<EntityUid>> CannonGroups = new() { { "all", new() } };
    public string CurrentGroupName = "all";
    public List<EntityUid> CurrentGroup => CannonGroups[CurrentGroupName];

    public bool RegenerateCannons = true;
    public TargetingConsoleBoundUserInterfaceState? PrevState;
}