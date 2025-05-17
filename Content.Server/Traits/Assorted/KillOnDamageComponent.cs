namespace Content.Server.Traits.Assorted;

/// <summary>
/// Kills an entity when it takes a certain amount of a certain damage type in a single instance.
/// Damage type required, minimum damage of that type required to kill, and message shown on death are customisable.
/// </summary>
[RegisterComponent, Access(typeof(KillOnDamageSystem))]
public sealed partial class KillOnDamageComponent : Component
{
    [DataField]
    public string DamageType = "Shock";

    [DataField]
    public float Threshold = 5.0f;

    [DataField]
    public LocId Popup = "fragileCircuits-kill-popup";
}
