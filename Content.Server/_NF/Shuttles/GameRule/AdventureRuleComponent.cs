using System.ComponentModel.DataAnnotations;
using Content.Server.GameTicking.Configuration;
using Robust.Shared.Audio;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(NfAdventureRuleSystem))]
public sealed partial class AdventureRuleComponent : Component
{
    [DataField(required: true)]
    public string GamemodeName = "";

    [DataField("gameMaps", required: true)]
    public Dictionary<string, HullrotMapElement> GameMaps = new();

    /// <summary>
    /// The gameMaps this gamemode contains.
    /// </summary>
    //public IReadOnlyDictionary<string, HullrotMapElement> GameMaps => _gameMaps;
}
