using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._LostParadise.Clothing;

/// <summary>
/// Made by BL02DL from _LostParadise
/// </summary>

[RegisterComponent, NetworkedComponent(), AutoGenerateComponentState]
[Access(typeof(SharedNightVisionSystem))]
public sealed partial class NightVisionComponent : Component
{
    [DataField]
    public EntProtoId ToggleAction = "LPPActionToggleNightVision";

    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;

    [DataField("on"), AutoNetworkedField]
    public bool On;

    [DataField("tint1"), ViewVariables(VVAccess.ReadWrite)]
    public float Tint1 { get; set; } = 0.3f;

    [DataField("tint2"), ViewVariables(VVAccess.ReadWrite)]
    public float Tint2 { get; set; } = 0.3f;

    [DataField("tint3"), ViewVariables(VVAccess.ReadWrite)]
    public float Tint3 { get; set; } = 0.3f;

    [DataField("tint"), ViewVariables(VVAccess.ReadWrite)]
    public Vector3 Tint
    {
        get => new(Tint1, Tint2, Tint3);
        set
        {
            Tint1 = value.X;
            Tint2 = value.Y;
            Tint3 = value.Z;
        }
    }

    [DataField("strength"), ViewVariables(VVAccess.ReadWrite)]
    public float Strength = 2f;

    [DataField("noise"), ViewVariables(VVAccess.ReadWrite)]
    public float Noise = 0.5f;
}
