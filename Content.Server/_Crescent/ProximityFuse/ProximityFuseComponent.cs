namespace Content.Server._Crescent.ProximityFuse;

[RegisterComponent]
public sealed partial class ProximityFuseComponent : Component
{
    [DataField]
    public float MaxRange = 25f; 

    [DataField]
    public float MinRange = 5f;

    public float Fuse = 0.05f;

    public float SafetyTime = 0f;
}
