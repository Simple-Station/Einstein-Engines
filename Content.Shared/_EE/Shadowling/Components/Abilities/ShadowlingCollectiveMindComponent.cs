using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for the Collective Mind ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingCollectiveMindComponent : Component
{
    public string? ActionCollectiveMind = "ActionCollectiveMind";

    // <summary>
    // Contains all actions that are locked behind Thrall requirements.
    // Removes actions from here once they are gained
    // int: Required Thralls
    // ProtoId: Action
    // </summary>
    [DataField]
    public Dictionary<int, ProtoId<EntityPrototype>> LockedActions = new()
    {
        { 5, "ActionBlindnessSmoke"},
        { 7, "ActionNullCharge"},
    };

    [DataField]
    public Dictionary<int, string> ActionComponentNames = new()
    {
        { 5, "ShadowlingBlindnessSmoke" },
        { 7, "ShadowlingNullCharge"},
    }; // Le shitcode

    [DataField]
    public int AmountOfThralls;

    [DataField]
    public int ThrallsRequiredForAscension = 20;

    [DataField]
    public float BaseStunTime = 0.5f;
}
