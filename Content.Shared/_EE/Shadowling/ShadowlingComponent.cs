using System.Timers;
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

    // Phase Indicator
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public ShadowlingPhases CurrentPhase = ShadowlingPhases.PreHatch;

    [DataField]
    public bool IsHatching;

    [DataField]
    public TimeSpan HatchTimer = TimeSpan.FromSeconds(5);

    [DataField]
    public Color EyeColor = Color.FromHex("#f80000");
}

[NetSerializable, Serializable]
public enum ShadowlingPhases : byte
{
    PreHatch,
    PostHatch,
    Ascension,
}
