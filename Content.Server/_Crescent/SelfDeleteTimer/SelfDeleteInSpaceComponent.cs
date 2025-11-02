namespace Content.Server._Crescent.HullrotSelfDeleteTimer;

[RegisterComponent]
public sealed partial class SelfDeleteInSpaceComponent : Component
{
    /// <summary>
    /// Time it takes for this ENTITY to delete itself, once it's parented to space.
    ///
    [DataField]
    public TimeSpan TimeToDelete = TimeSpan.FromSeconds(10);

}
