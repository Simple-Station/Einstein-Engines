using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;


namespace Content.Shared._EE.Shadowling;


[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ShadowlingComponent : Component
{
    // The round-start Shadowling Actions
    public readonly List<ProtoId<EntityPrototype>> BaseShadowlingActions = new()
    {
        "ActionHatch",
    };

    public readonly List<ProtoId<EntityPrototype>> PostHatchShadowlingActions = new()
    {
        "ActionEnthrall",
        "ActionGlare",
    };

    // The status icon for Shadowlings
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "ShadowlingFaction";

    // Phase Indicator
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public ShadowlingPhases CurrentPhase = ShadowlingPhases.PreHatch;

    [DataField]
    public bool IsHatching;

    [DataField]
    public Color EyeColor = Color.FromHex("#f80000");

    [DataField]
    public Color SkinColor = Color.FromHex("#000000");

    [DataField]
    public string Egg = "SlingEgg";

    // Thrall Indicator
    [DataField]
    public List<EntityUid> Thralls = new();
}

[NetSerializable, Serializable]
public enum ShadowlingPhases : byte
{
    PreHatch,
    PostHatch,
    Ascension,
}
