// <summary>
//   Locks an item to only be pick-upable by onis.
// </summary>
[RegisterComponent]
public sealed partial class OniOnlyComponent : Component
{
    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(2);
}
