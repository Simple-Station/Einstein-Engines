using Content.Shared.Aliens.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(AcidMakerSystem)), AutoGenerateComponentState]
public sealed partial class AcidMakerComponent : Component
{
    /// <summary>
    /// The text that pops up whenever making acid fails for not having enough plasma.
    /// </summary>
    [DataField("popupText")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public string PopupText = "alien-action-fail-plasma";

    /// <summary>
    /// What will be produced at the end of the action.
    /// </summary>
    [DataField(required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public EntProtoId EntityProduced;

    /// <summary>
    /// The entity needed to actually make acid. This will be granted (and removed) upon the entity's creation.
    /// </summary>
    [DataField(required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public EntProtoId Action;

    [AutoNetworkedField]
    [DataField("actionEntity")]
    public EntityUid? ActionEntity;

    /// <summary>
    /// This will subtract (not add, don't get this mixed up) from the current plasma of the mob making acid.
    /// </summary>
    [DataField("plasmaCost")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float PlasmaCost = 150f;

}
