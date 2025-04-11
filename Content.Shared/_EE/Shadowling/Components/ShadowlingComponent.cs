using System.Timers;
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

    // Cooldown Timers
    [DataField]
    public TimeSpan EnthrallTime = TimeSpan.FromSeconds(1.3); // this needs actual playtesting

    #region Glare
    // <summary>
    // Variable stun time. On distance 1 or lower, it is maximized to 4 seconds of stun (enough to Enthrall),
    // otherwise it gets reduced based on distance.
    // </summary>
    [DataField]
    public float GlareStunTime;

    // <summary>
    // Variable activation time. On distance 1 or lower, it is immediate,
    // otherwise it gets increased based on distance.
    // Max time before stun is 2 seconds
    // </summary>
    [DataField]
    public float GlareTimeBeforeEffect;

    [DataField]
    public float MaxGlareDistance = 10f;

    [DataField]
    public float MinGlareDistance = 1f;

    [DataField]
    public float MaxGlareStunTime = 4f;

    [DataField]
    public float MaxGlareDelay = 2f;

    [DataField]
    public float MinGlareDelay = 0.1f;

    [DataField]
    public float MuteTime = 2f;

    [DataField]
    public float SlowTime = 2f;

    [DataField]
    public EntityUid GlareTarget;
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

    // Action Related
    [DataField]
    public float GlareDistance;

    public bool ActivateGlareTimer;
}

[NetSerializable, Serializable]
public enum ShadowlingPhases : byte
{
    PreHatch,
    PostHatch,
    Ascension,
}
