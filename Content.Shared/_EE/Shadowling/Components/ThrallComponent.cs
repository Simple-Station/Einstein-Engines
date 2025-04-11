using Content.Shared._EE.Shadowling.Systems;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for marking Thralls and storing their icons
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedShadowlingSystem))]
public sealed partial class ThrallComponent : Component
{

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "ThrallFaction";

    public readonly List<ProtoId<EntityPrototype>> BaseThrallActions = new()
    {
        "ActionThrallDarksight"
    };

    [DataField]
    public bool NightVisionMode;

    /// <summary>
    /// Sound that plays when you are chosen as Thrall. (Need to find it)
    /// </summary>
    // [DataField]
    // public SoundSpecifier ThrallConverted = new SoundPathSpecifier();


}
