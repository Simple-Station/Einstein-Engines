namespace Content.Server._Crescent.HullrotSelfDeleteTimer;

[RegisterComponent]
public sealed partial class SelfDeleteGridComponent : Component
{
    /// <summary>
    /// Time it takes to delete this GRID. Does NOT work for entities.
    /// used for auto-deleting grids on hullrot for lag cleanup
    /// </summary>
    [DataField]
    public TimeSpan TimeToDelete = TimeSpan.FromMinutes(10); //default value MUST be 10 minutes because that's how long we need it to be after it's EnsureComp'd (it takes this default value)

}
