using Robust.Shared.Serialization;

namespace Content.Shared.Silicons.StationAi
{

    [Serializable, NetSerializable]
    public sealed class StationAiInfoUpdateState : BoundUserInterfaceState
    {
        public string AiName;
        public string? StationName;
        public string? StationAlertLevel;
        public Color StationAlertColor;
        public StationAiInfoUpdateState(string aIName, string? stationName, string? stationAlertLevel, Color stationAlertColor)
        {
            AiName = aIName;
            StationName = stationName;
            StationAlertLevel = stationAlertLevel;
            StationAlertColor = stationAlertColor;
        }
    }
}
