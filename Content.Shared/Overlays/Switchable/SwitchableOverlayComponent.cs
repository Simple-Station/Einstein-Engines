using System.Dynamic;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Overlays.Switchable;

public abstract partial class SwitchableOverlayComponent : BaseOverlayComponent
{
    [DataField, AutoNetworkedField]
    public bool IsActive;

    [DataField]
    public bool DrawOverlay = true;

    /// <summary>
    /// Whether it should grant equipment enhanced vision or is it mob vision
    /// </summary>
    [DataField]
    public bool IsEquipment;


    [DataField]
    public float PulseTime = 0;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan PulseAccumulator = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan PulseEndTime = TimeSpan.Zero;

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
