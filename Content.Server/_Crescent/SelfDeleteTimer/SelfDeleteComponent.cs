namespace Content.Server._Crescent.HullrotSelfDeleteTimer;

[RegisterComponent]
public sealed partial class SelfDeleteComponent : Component
{
    /// <summary>
    /// Time it takes for this ENTITY to delete itself. Does NOT work for grids.
    ///
    [DataField]
    public TimeSpan TimeToDelete = TimeSpan.FromSeconds(10);

}
