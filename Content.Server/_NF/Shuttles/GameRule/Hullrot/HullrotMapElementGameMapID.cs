using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Content.Server.Maps.NameGenerators;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Configuration;

/// <summary>
/// A config for a hullrot map element, defined by a gameMap prototype's ID. These are usually stations like Vladzena.
/// </summary>
[DataDefinition, PublicAPI]
public sealed partial class HullrotMapElementGameMapID
{
    /// <summary>
    /// This string matches the specific gameMap prototype's ID field. This tells the game what thing to actually spawn, and gets the path from that prototype too.
    /// </summary>
    [DataField("gameMapID", required: true)]
    public string GameMapID = "";

    [DataField("posX", required: true)]
    public float PositionX = 0f;

    [DataField("posY", required: true)]
    public float PositionY = 0f;

    [DataField("IFFColor", required: false)]
    public Color IFFColor = Color.White;

    [DataField("HideIFF", required: false)]
    public bool HideIFF = false;

    [DataField("forcedName", required: false)]
    public string? ForcedName = null;

    /// <summary>
    /// This float decides the maximum random offset for X for this map element when it spawns. Leave unconfigured or at 0 if you want it fixed.
    /// </summary>
    [DataField("randomOffsetX", required: false)]
    public float RandomOffsetX = 0f;

    /// <summary>
    /// This float decides the maximum random offset for y for this map element when it spawns. Leave unconfigured or at 0 if you want it fixed.
    /// </summary>
    [DataField("randomOffsetY", required: false)]
    public float RandomOffsetY = 0f;

    /// <summary>
    /// This string sets the IFF for this particular object. Leave "null" to not modify IFF.
    /// </summary>
    [DataField("IFFFaction", required: false)]
    public string? IFFFaction = null;

}
