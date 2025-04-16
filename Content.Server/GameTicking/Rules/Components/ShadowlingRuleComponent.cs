using Content.Shared.Store;
using Robust.Shared.Prototypes;


namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(ShadowlingRuleSystem))]
public sealed partial class ShadowlingRuleComponent : Component
{
    [DataField]
    public Color EyeColor = Color.FromHex("#f80000");
}
