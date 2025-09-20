namespace Content.Server._Crescent.ProximityFuse;

[RegisterComponent]
public sealed partial class ProximityFuseComponent : Component
{
    [DataField]
    public float MaxRange = 10f;

    [DataField]
    public float Safety = 0.5f;

    [DataField]
    public List<Target> Targets = new();
}

public class Target
{
    public EntityUid ent { get; set; }
    public float Distance { get; set; }
    public float LastDistance { get; set; }
}
