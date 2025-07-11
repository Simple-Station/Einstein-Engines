using Robust.Shared.GameStates;
using Robust.Shared.Utility;


namespace Content.Shared._Crescent.Vessel;


/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VesselIconComponent : Component
{
    [DataField("iffIcon"), AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public SpriteSpecifier? Icon;
}
