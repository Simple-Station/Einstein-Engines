using Robust.Shared.Utility;


namespace Content.Shared._Crescent.Vessel;


/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class VesselIconComponent : Component
{
    [DataField("iffIcon")] public SpriteSpecifier? Icon;
}
