using Robust.Shared.GameStates;

namespace Content.Shared.PointCannons;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PointCannonComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public List<(Angle, Angle)> ObstructedRanges = new();

    /// <summary>
    /// Since projectiles vary in size and it's kinda hard to estimate how much more clearance is needed
    /// to prevent large projectiles from colliding with walls, you should set this manually
    /// Only used when generating safety ranges
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Angle ClearanceAngle = 0;

    /// <summary>
    ///  Yes this caused the entire server to black-out for everyone when multiple consoles had the same cannon that got deleted and wasn't properly removed
    /// </summary>
    [DataField,ViewVariables(VVAccess.ReadOnly), Obsolete]
    public EntityUid? LinkedConsoleId;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> LinkedConsoleIds = new();
}
