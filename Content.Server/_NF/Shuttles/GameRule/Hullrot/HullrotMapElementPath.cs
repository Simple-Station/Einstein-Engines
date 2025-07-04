using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Content.Server.Maps.NameGenerators;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Configuration;

/// <summary>
/// A config for a hullrot map element, defined by a path. These are usually asteroid fields or neutral Points Of Interest.
/// </summary>
[DataDefinition, PublicAPI]
public sealed partial class HullrotMapElementPath
{

    [DataField("path", required: true)]
    public string Path = "";

    [DataField("entityName", required: true)]
    public string EntityName = "";

    [DataField("posX", required: true)]
    public float PositionX = 0f;

    [DataField("posY", required: true)]
    public float PositionY = 0f;

    [DataField("IFFColor", required: false)]
    public Color IFFColor = Color.White;

    [DataField("HideIFF", required: false)]
    public bool HideIFF = false;

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

}