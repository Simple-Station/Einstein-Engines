namespace Content.Server._Crescent.SelfDeleteTimer;

[RegisterComponent]
public sealed partial class SelfDeleteGridComponent : Component
{
    /// <summary>
    /// Time it takes to delete this GRID. Does NOT work for entities.
    /// </summary>
    [DataField]
    public TimeSpan TimeToDelete = TimeSpan.FromSeconds(10);

}
