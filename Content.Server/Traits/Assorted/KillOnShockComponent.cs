namespace Content.Server.Traits.Assorted;

/// <summary>
/// Kills an entity when it takes a certain amount of a certain damage type at once.
/// </summary>
[RegisterComponent, Access(typeof(KillOnShockSystem))]
public sealed partial class KillOnShockComponent : Component
{
    [DataField]
    public string Type = "Shock";

    [DataField]
    public float Threshold = 5.0f;

    [DataField]
    public LocId Popup = "fragileCircuits-kill-popup";
}
