using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Client._ES.Lighting;

/// <summary>
///     This is used for showing a clientside pointlight for your own controlled mob, like in SS13.
/// </summary>
[RegisterComponent]
public sealed partial class ESInherentLightComponent : Component
{
    [ViewVariables]
    public EntityUid? LightEntity = null;

    /// <summary>
    ///     Prototype to spawn as the light entity.
    ///     Should obviously have a pointlight of some kind, with netsync false, and disabled by default.
    /// </summary>
    [DataField]
    public EntProtoId LightPrototype = "ESMobDefaultInherentLight";

    [DataField]
    public bool Enabled = true;
}
