using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Overlays.Switchable;

public abstract partial class SwitchableOverlayComponent : BaseOverlayComponent
{
    [DataField, AutoNetworkedField]
    public virtual bool IsActive { get; set; }

    [DataField]
    public virtual bool DrawOverlay { get; set; } = true;

    /// <summary>
    /// Whether it should grant equipment enhanced vision or is it mob vision
    /// </summary>
    [DataField]
    public virtual bool IsEquipment { get; set; }

    /// <summary>
    /// If it is greater than 0, overlay isn't toggled but pulsed instead
    /// </summary>
    [DataField]
    public virtual float PulseTime { get; set; }

    [ViewVariables(VVAccess.ReadOnly)]
    public float PulseAccumulator;

    [DataField]
    public virtual float FlashDurationMultiplier { get; set; } = 1f; // ! goober

    [DataField]
    public virtual SoundSpecifier? ActivateSound { get; set; } =
        new SoundPathSpecifier("/Audio/Items/Goggles/activate.ogg");

    [DataField]
    public virtual SoundSpecifier? DeactivateSound { get; set; } =
        new SoundPathSpecifier("/Audio/Items/Goggles/deactivate.ogg");

    [DataField]
    public virtual string? ToggleAction { get; set; }

    [ViewVariables]
    public EntityUid? ToggleActionEntity;
}

[Serializable, NetSerializable]
public sealed class SwitchableVisionOverlayComponentState : IComponentState
{
    public Color Color;
    public bool IsActive;
    public SoundSpecifier? ActivateSound;
    public SoundSpecifier? DeactivateSound;
    public EntProtoId? ToggleAction;
    public float LightRadius;
}
