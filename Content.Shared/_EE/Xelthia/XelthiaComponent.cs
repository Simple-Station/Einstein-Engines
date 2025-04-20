using Robust.Shared.GameStates;


namespace Content.Shared._EE.Xelthia;


/// <summary>
/// This is eventually going to be used for the arm regrowth action the Xelthia have. I have no idea how to code this.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class XelthiaComponent : Component
{
    /// <summary>
    /// These should store the colors for the Xelthia's spikes when regrown? I'm unsure how necessary this strictly is
    /// but I imagine I'll need to have this for the color to persist.
    /// </summary>
    [DataField]
    public TimeSpan UseDelay { get; set; } = TimeSpan.FromMinutes(5);

    [DataField]
    public EntityUid? XelthiaRegenerateAction;
}
