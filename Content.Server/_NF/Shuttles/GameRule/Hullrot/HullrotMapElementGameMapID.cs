using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Content.Server.Maps.NameGenerators;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Configuration;

/// <summary>
/// A config for a hullrot map element. Any grid that spawns when a gamemode starts.
/// </summary>
[DataDefinition, PublicAPI]
public sealed partial class HullrotMapElementGameMapID
{
    [DataField("gameMapID", required: true)]
    public string GameMapID = "";

    [DataField("posX", required: true)]
    public float PositionX = 0f;

    [DataField("posY", required: true)]
    public float PositionY = 0f;

}