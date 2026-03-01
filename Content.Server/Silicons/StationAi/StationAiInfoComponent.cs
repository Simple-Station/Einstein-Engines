using Robust.Shared.Prototypes;

namespace Content.Server.Silicons.StationAi;

[RegisterComponent]
public sealed partial class StationAiInfoComponent : Component
{
    [DataField]
    public EntityUid? Target;
    [DataField("Info")]
    public EntProtoId Action = "ActionStationAiInfo";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [ViewVariables]
    public string? StationName;
    [ViewVariables]
    public string? StationAlertLevel;
    [ViewVariables]
    public Color StationAlertColor = Color.White;

}
