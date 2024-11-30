using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Shared.CCVar;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Server.DynamicHostname;


/// <summary>
/// This handles dynamically updating hostnames.
/// </summary>
public sealed class DynamicHostnameSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IGameMapManager _mapManager = default!;

    private readonly DynamicHostnameData _hostnameData = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        // Must be set at server start to run.
        if (!_configuration.GetCVar(CCVars.UseDynamicHostname))
            return;

        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
        SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
        SubscribeLocalEvent<RoundEndedEvent>(OnRoundEnded);

        _hostnameData.OriginalHostname = _configuration.GetCVar(CVars.GameHostName);
        UpdateHostname();
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        _hostnameData.CurrentPresetName = _gameTicker.CurrentPreset?.ModeTitle;
        _hostnameData.CurrentMapName = _mapManager.GetSelectedMap()?.MapName;

        UpdateHostname();
    }

    private void OnRoundStarted(RoundStartedEvent ev)
    {
        _hostnameData.CurrentPresetName = _gameTicker.CurrentPreset?.ModeTitle;
        _hostnameData.CurrentMapName = _mapManager.GetSelectedMap()?.MapName;

        UpdateHostname();
    }

    private void OnRoundEnded(RoundEndedEvent ev)
    {
        _hostnameData.CurrentMapName = null;
        _hostnameData.CurrentPresetName = null;

        UpdateHostname();
    }

    private string GetLocId()
    {
        switch (_gameTicker.RunLevel)
        {
            case GameRunLevel.PreRoundLobby:
                return "in-lobby";
            case GameRunLevel.InRound:
                return "in-round";
            case GameRunLevel.PostRound:
                return "post-round";
            default:
                return "in-lobby";
        }
    }

    private void UpdateHostname()
    {
        var locId = GetLocId();
        var presetName = "No preset";

        if (_hostnameData.CurrentPresetName != null)
            presetName = Loc.GetString(_hostnameData.CurrentPresetName);

        var hostname = Loc.GetString($"dynamic-hostname-{locId}-hostname",
            ("originalHostName", _hostnameData.OriginalHostname),
            ("preset", presetName),
            ("mapName", _hostnameData.CurrentMapName ?? "No map"));

        _configuration.SetCVar(CVars.GameHostName, hostname);
    }
}

public sealed class DynamicHostnameData
{
    public string OriginalHostname { get; set; } = "";
    public string? CurrentMapName { get; set; }
    public string? CurrentPresetName { get; set; }
}
