namespace Content.Server._Crescent.Shipyard;

/// <summary>
/// Used by shipyard to stop people from immediately stealing a ship or taking someone else's.
/// </summary>
[RegisterComponent]
public sealed partial class PurchaseLockoutComponent : Component
{
    public TimeSpan CreationTime;
    public TimeSpan LockoutTime = TimeSpan.FromMinutes(3);
    [DataField]
    public string? Purchaser = null;
}
