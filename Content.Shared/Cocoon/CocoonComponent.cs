namespace Content.Shared.Cocoon;

[RegisterComponent]
public sealed partial class CocoonComponent : Component
{
    public string? OldAccent;

    public EntityUid? Victim;

    [DataField]
    public float DamagePassthrough = 0.5f;

}
