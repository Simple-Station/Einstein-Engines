using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, AutoGenerateComponentState]
public sealed partial class ResinSpinnerComponent : Component
{
    [DataField("popupText")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public string PopupText = "alien-action-fail-plasma";

    /// <summary>
    /// How long will it take to make.
    /// </summary>
    [DataField("productionLengthWall")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float ProductionLengthWall = 2f;

    /// <summary>
    /// This will subtract (not add, don't get this mixed up) from the current plasma of the mob making structure.
    /// </summary>
    [DataField("plasmaCostWall")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float PlasmaCostWall = 60f;

    /// <summary>
    /// The wall prototype to use.
    /// </summary>
    [DataField("wallPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string WallPrototype = "WallResin";

    [DataField("resinWallAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? ResinWallAction = "ActionAlienDroneWall";

    [DataField("resinWallActionEntity")] public EntityUid? ResinWallActionEntity;

    /// <summary>
    /// How long will it take to make.
    /// </summary>
    [DataField("productionLengthWindow")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float ProductionLengthWindow = 2f;

    /// <summary>
    /// This will subtract (not add, don't get this mixed up) from the current plasma of the mob making structure.
    /// </summary>
    [DataField("plasmaCostWindow")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float PlasmaCostWindow = 60f;

    /// <summary>
    /// The wall prototype to use.
    /// </summary>
    [DataField("windowPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string WindowPrototype = "WindowResin";

    [DataField("resinWindowAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? ResinWindowAction = "ActionWindowResin";

    [DataField("resinWindowActionEntity")] public EntityUid? ResinWindowActionEntity;
}
