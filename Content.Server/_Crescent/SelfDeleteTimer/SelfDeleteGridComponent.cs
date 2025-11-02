namespace Content.Server._Crescent.SelfDeleteTimer;

[RegisterComponent]
public sealed partial class SelfDeleteGridComponent : Component
{
    /// <summary>
    /// Time it takes to delete this GRID. Does NOT work for entities.
    /// used for auto-deleting grids on hullrot for lag cleanup
    /// </summary>
    [DataField]
    public TimeSpan TimeToDelete = TimeSpan.FromMinutes(20); //default value MUST be 20 minutes because that's how long we need it to be EnsureComp'd

}
