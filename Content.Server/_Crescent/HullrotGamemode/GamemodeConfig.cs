using Content.Server.Maps.NameGenerators;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._Crescent.GameMode;

/// <summary>
/// A config for a gamemode. Specifies name and component modifications.
/// </summary>
[DataDefinition, PublicAPI]
public sealed partial class GamemodeConfig
{
    [DataField("gamemodeProto", required: true)]
    public string GamemodePrototype = default!;

    [DataField("components", required: true)]
    public ComponentRegistry GamemodeComponentOverrides = default!;
}

