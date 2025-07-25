namespace Content.Server.Teleportation;

[RegisterComponent]
public sealed partial class SquashTeleportComponent : Component
{
    /// <summary>
    ///     Teleportation radius.
    /// </summary>
    [DataField]
    public float TeleportRadius = 10f;
}
