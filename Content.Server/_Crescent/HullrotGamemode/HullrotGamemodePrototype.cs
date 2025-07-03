using Content.Server.Station;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Diagnostics;


namespace Content.Server._Crescent.HullrotGamemode;

/// <summary>
/// Prototype data for a gamemode, hullrot specific.
/// </summary>
[Prototype("hullrotGamemode"), PublicAPI]
[DebuggerDisplay("HullrotGamemode [{ID}]")]
public sealed partial class HullrotGamemode : IPrototype
{
    /// <inheritdoc/>
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
