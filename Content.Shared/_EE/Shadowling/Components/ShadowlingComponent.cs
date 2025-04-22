using Content.Shared.Alert;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;


namespace Content.Shared._EE.Shadowling;

// <summary>
// Handles the main actions of a Shadowling
// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ShadowlingComponent : Component
{
    // The round-start Shadowling Actions
    public string ActionHatch = "ActionHatch";
    public EntityUid? ActionHatchEntity;

    #region PostHatch Actions
    public string ActionEnthrall       = "ActionEnthrall";
    public string ActionGlare          = "ActionGlare";
    public string ActionVeil           = "ActionVeil";
    public string ActionRapidRehatch   = "ActionRapidRehatch";
    public string ActionShadowWalk     = "ActionShadowWalk";
    public string ActionIcyVeins       = "ActionIcyVeins";
    public string ActionDestroyEngines = "ActionDestroyEngines";
    public string ActionCollectiveMind = "ActionCollectiveMind";
    public string ActionAscendance     = "ActionAscendance"; // remove once debugged

    public EntityUid? ActionEnthrallEntity;
    public EntityUid? ActionGlareEntity;
    public EntityUid? ActionVeilEntity;
    public EntityUid? ActionRapidRehatchEntity;
    public EntityUid? ActionShadowWalkEntity;
    public EntityUid? ActionIcyVeinsEntity;
    public EntityUid? ActionDestroyEnginesEntity;
    public EntityUid? ActionCollectiveMindEntity;
    public EntityUid? ActionAscendanceEntity; // remove once debugged
    #endregion

    #region Ascension Actions
    public string ActionAnnihilate      = "ActionAnnihilate";
    public string ActionHypnosis        = "ActionHypnosis";
    public string ActionPlaneShift      = "ActionPlaneShift";
    public string ActionLightningStorm  = "ActionLightningStorm";
    public string ActionBroadcast       = "ActionAscendantBroadcast";

    public EntityUid? ActionAnnihilateEntity;
    public EntityUid? ActionHypnosisEntity;
    public EntityUid? ActionPlaneShiftEntity;
    public EntityUid? ActionLightningStormEntity;
    public EntityUid? ActionBroadcastEntity;
    #endregion

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

    [DataField]
    public ProtoId<AlertPrototype> AlertProto = "ShadowlingLight";

    [DataField]
    public int AlertSprites = 11;
}

[NetSerializable, Serializable]
public enum ShadowlingPhases : byte
{
    PreHatch,
    PostHatch,
    Ascension,
}
