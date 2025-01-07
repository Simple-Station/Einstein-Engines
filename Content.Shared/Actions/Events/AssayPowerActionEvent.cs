using Robust.Shared.Audio;

namespace Content.Shared.Actions.Events;
public sealed partial class AssayPowerActionEvent : EntityTargetActionEvent
{
    [DataField]
    public TimeSpan UseDelay = TimeSpan.FromSeconds(8f);

    [DataField]
    public SoundSpecifier SoundUse = new SoundPathSpecifier("/Audio/Psionics/heartbeat_fast.ogg");

    [DataField]
    public string PopupTarget = "assay-begin";

    [DataField]
    public int FontSize = 10;

    [DataField]
    public Color FontColor = Color.White;

    [DataField]
    public int MinGlimmer = 3;

    [DataField]
    public int MaxGlimmer = 6;

    [DataField]
    public string PowerName = "assay";
}
