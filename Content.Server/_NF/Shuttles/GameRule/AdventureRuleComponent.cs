using System.ComponentModel.DataAnnotations;
using Content.Server.GameTicking.Configuration;
using Robust.Shared.Audio;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(NfAdventureRuleSystem))]
public sealed partial class AdventureRuleComponent : Component
{
    [DataField("gameMapsByID", required: false)]
    public Dictionary<string, HullrotMapElementGameMapID> GameMapsID = new();

}
