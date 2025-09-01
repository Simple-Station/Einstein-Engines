using Robust.Shared.Player;


namespace Content.Shared._Crescent.PvsAutoOverride;


/// <summary>
/// Shared Component for prototype purposes!
/// </summary>
[RegisterComponent]
public sealed partial class PvsAutoOverrideComponent : Component
{
    [DataField]
    public float PvsSendRange = 100f;
    [NonSerialized] // this is only filled server-side , i would've split into a partial component if it was possible SPCR 2025
    public List<ICommonSession> SendingSessions = new List<ICommonSession>();
}
