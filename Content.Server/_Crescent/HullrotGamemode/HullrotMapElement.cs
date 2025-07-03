using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Content.Server.Maps.NameGenerators;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._Crescent.HullrotGamemode;

/// <summary>
/// A config for a hullrot map element. Any grid that spawns when a gamemode starts.
/// </summary>
[DataDefinition, PublicAPI]
public sealed partial class HullrotMapElement
{
    [DataField("gameMapID", required: true)]
    public string GameMapID = default!;

    [DataField("posX", required: true)]
    public float PositionX = 0f;

    [DataField("posY", required: true)]
    public float PositionY = 0f;

    // [DataField("shuttleName", required: false)]
    // public string ShuttleName = "";
}

