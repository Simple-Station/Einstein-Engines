namespace Content.Shared.Repulsor;

[RegisterComponent]
public sealed partial class RepulseComponent : Component
{
    [DataField]
    public float ForceMultiplier = 13000;

    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(3);

    [DataField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(3);
}

public sealed class BeforeRepulseEvent(EntityUid origin, EntityUid target) : CancellableEntityEventArgs
{
    public EntityUid Origin = origin;
    public EntityUid Target = target;
}
