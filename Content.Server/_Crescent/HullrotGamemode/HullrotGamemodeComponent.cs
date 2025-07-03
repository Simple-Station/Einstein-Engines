using Content.Server.Station;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Diagnostics;


namespace Content.Server._Crescent.HullrotGamemode.Components;

/// <summary>
/// Component containing every grid to be spawned in a specific hullrot gamemode. To be attached to a GameRule in hullrotGamemodes.yml.
/// </summary>
public sealed partial class HullrotGamemodeComponent : Component
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Name of the gamemode to use in generic messages, like the gamemode vote.
    /// </summary>
    [DataField("gamemodeName", required: true)]
    public string GamemodeName { get; private set; } = default!;

    [DataField("gameMaps", required: true)]
    private Dictionary<string, HullrotMapElement> _gameMaps = new();

    /// <summary>
    /// The gameMaps this gamemode contains.
    /// </summary>
    public IReadOnlyDictionary<string, HullrotMapElement> GameMaps => _gameMaps;


}
