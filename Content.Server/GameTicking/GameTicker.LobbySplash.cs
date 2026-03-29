using Content.Shared.CCVar;
using Content.Shared.Dataset;
using Content.Shared.GameTicking.Prototypes;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server.GameTicking;

public sealed partial class GameTicker
{
    [ViewVariables]
    public string? LobbySplashText;
    const string _datasetPrototype = "LobbySplashText";

    private void UpdateSplashText()
    {
        if(!_cfg.GetCVar(CCVars.EnableSplash) ||
           !_prototypeManager.TryIndex<LocalizedDatasetPrototype>(_datasetPrototype, out var prototype))
        {
            LobbySplashText = null;
            return;
        }
        LobbySplashText = _robustRandom.Pick(prototype.Values);
    }
}
